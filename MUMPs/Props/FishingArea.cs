using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

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
                new(OpCodes.Call, typeof(FishingArea).MethodNamed(nameof(SwapPool))),
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
                defaultRegionName.Value = defaults.Length > 0 ? defaults[0] : null;
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

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFish))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> GetFish(IEnumerable<CodeInstruction> instructions) => fishPatch.Run(instructions);

        [HarmonyPatch(typeof(Farm), nameof(Farm.getFish))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> GetFarmFish(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction prev = null;
            var getFish = typeof(GameLocation).MethodNamed(nameof(GameLocation.getFish));
            var swapFish = typeof(FishingArea).MethodNamed(nameof(FarmSwapPool));
            foreach (var code in instructions)
            {
                if (prev is not null) 
                {
                    if (code.opcode == OpCodes.Call && (code.operand as MethodInfo) == getFish)
                    {
                        yield return new(OpCodes.Ldarg_S, 6);
                        yield return prev;
                        yield return new(OpCodes.Call, swapFish);
                        yield return code;
                    } else
                    {
                        yield return prev;
                        yield return code;
                    }
                    prev = null;
                }
                else if (code.opcode == OpCodes.Ldstr)
                {
                    prev = code;
                }
                else
                {
                    yield return code;
                }
            }
        }
        private static string SwapPool(Vector2 bobber, string location)
        {
            if (location != null)
                return location;

            foreach ((var region, string loc) in locRegions.Value)
                if (region.Contains(bobber))
                    return loc;
            return defaultRegionName.Value;
        }
        private static string FarmSwapPool(Vector2 bobber, string location)
        {
            foreach ((var region, string loc) in locRegions.Value)
                if (region.Contains(bobber))
                    return loc;
            return location ?? defaultRegionName.Value;
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFish))]
        [HarmonyPostfix]
        internal static void CatchTreasure(Vector2 bobber, GameLocation __instance, Farmer who, ref SObject __result)
        {
            var prop = __instance.doesTileHaveProperty((int)bobber.X, (int)bobber.Y, "FishingTreasure", "Back");
            if (prop is null)
                return;
            var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1 || !split[0].TryGetItem(out Item fished) || fished is not SObject fobj)
                return;

            Farmer address = split.Length < 3 ? Game1.MasterPlayer : who; // whether instanced or not
            if (split.Length < 2)
            {
                __result = fobj;
            }
            else if (!address.hasOrWillReceiveMail(split[1]))
            {
                if (split.Length > 2)
                    address.mailForTomorrow.Add(split[1]);
                else
                    address.mailReceived.Add(split[1]);
                __result = fobj;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFishingLocation))]
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
