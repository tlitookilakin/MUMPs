using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MUMPs.Props
{
    [HarmonyPatch]
    [ModInit]
    class FishingArea
    {
        internal static readonly PerScreen<Dictionary<Rectangle, int>> idRegions = new(() => new());
        internal static readonly PerScreen<Dictionary<Rectangle, string>> locRegions = new(() => new());
        internal static readonly PerScreen<int> defaultRegion = new(() => -1);
        internal static readonly PerScreen<string> defaultRegionName = new();
        private static readonly ILHelper fishPatch = new ILHelper(ModEntry.monitor, "GetFish")
            .Add(new CodeInstruction[]{
                new(OpCodes.Ldarg_S, 6),
                new(OpCodes.Ldarg_S, 7),
                new(OpCodes.Call, typeof(FishingArea).MethodNamed("SwapPool")),
                new(OpCodes.Starg_S, 7)
            })
            .Finish();

        internal static void Init()
        {
            ModEntry.OnChangeLocation += ChangeLocation;
            ModEntry.OnCleanup += Cleanup;
        }
        private static void ChangeLocation(GameLocation loc)
        {
            idRegions.Value.Clear();
            locRegions.Value.Clear();

            string[] data = Maps.MapPropertyArray(loc, "FishingAreaCorners");
            for (int i = 0; i + 4 < data.Length; i += 5)
            {
                if (data.FromCorners(out var rect, i))
                {
                    locRegions.Value[rect] = data[i + 4];
                    if (int.TryParse(data[i + 5], out int region))
                        idRegions.Value[rect] = region;
                }
            }
            string[] defaults = Maps.MapPropertyArray(loc, "DefaultFishingArea");
                defaultRegionName.Value = defaults.Length > 0 ? defaults[0] : loc.Name;
            defaultRegion.Value = defaults.Length > 1 && int.TryParse(defaults[1], out int def) ?
                 def : -1;
            ModEntry.monitor.Log($"Fishing: Found {idRegions.Value.Count} ID regions and {locRegions.Value.Count} Location regions.", LogLevel.Trace);
        }
        private static void Cleanup()
        {
            idRegions.ResetAllScreens();
            locRegions.ResetAllScreens();
            defaultRegion.ResetAllScreens();
            defaultRegionName.ResetAllScreens();
        }

        [HarmonyPatch(typeof(GameLocation), "getFish")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> GetFish(IEnumerable<CodeInstruction> instructions) => fishPatch.Run(instructions);

        [HarmonyPatch(typeof(Farm), "getFish")]
        [HarmonyPrefix]
        internal static bool FarmGetFish(Farm __instance, ref Object __result, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location)
        {
            if (bobberTile != Vector2.Zero)
                foreach (Building b in __instance.buildings)
                    if (b is FishPond && b.isTileFishable(bobberTile))
                    {
                        __result = (b as FishPond).CatchFish();
                        return false;
                    }
            location = SwapPool(bobberTile, location);
            
            if (location != null)
            {
                __result = FarmBaseGetFish(__instance, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, location);
                return false;
            }
            return true;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(GameLocation), "getFish")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Object FarmBaseGetFish(Farm instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, 
            double baitPotency, Vector2 bobberTile, string location = null) { return null; }
        private static string SwapPool(Vector2 bobber, string location)
        {
            if (location != null)
                return location;

            foreach ((var region, string loc) in locRegions.Value)
                if (region.Contains(bobber))
                    return loc;
            return defaultRegionName.Value;
        }
        [HarmonyPatch(typeof(GameLocation), "getFishingLocation")]
        [HarmonyPrefix]
        internal static bool GetFishingLocationPatch(ref Vector2 tile, ref int __result)
        {
            foreach((var region, int id) in idRegions.Value)
                if (region.Contains(tile))
                {
                    __result = id;
                    return false;
                }
            __result = defaultRegion.Value;
            return false;
        }
    }
}
