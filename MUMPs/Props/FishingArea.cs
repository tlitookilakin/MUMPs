using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class FishingArea
    {
        public static readonly PerScreen<Dictionary<Rectangle, int>> idRegions = new(() => new());
        public static readonly PerScreen<Dictionary<Rectangle, string>> locRegions = new(() => new());

        public static void ChangeLocation(GameLocation loc)
        {
            idRegions.Value.Clear();
            locRegions.Value.Clear();

            string[] data = Utils.MapPropertyArray(loc, "FishingRegions");
            for (int i = 0; i + 4 < data.Length; i += 5)
            {
                if (data.StringsToRect(out var rect, i))
                {
                    if (int.TryParse(data[i + 4], out int id))
                        idRegions.Value[rect] = id;
                    else
                        locRegions.Value[rect] = data[i + 4];
                } else
                {
                    ModEntry.monitor.Log("Invalid region specified in FishingRegions @ " + loc.Name + ".");
                    idRegions.Value.Clear();
                    locRegions.Value.Clear();
                    return;
                }
            }
        }

        public static void Cleanup()
        {
            idRegions.ResetAllScreens();
            locRegions.ResetAllScreens();
        }

        [HarmonyPatch(typeof(GameLocation), "getFish")]
        [HarmonyPrefix]
        public static void SwapFishingPool(ref Vector2 bobberTile, ref string location)
        {
            if (location != null)
                return;

            foreach((var region, string loc) in locRegions.Value)
            {
                if (region.Contains(bobberTile))
                {
                    location = loc;
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(Farm), "getFish")]
        [HarmonyPrefix]
        public static bool FarmSwapFishingPool(Farm __instance, Object __result, ref Vector2 bobberTile, ref string location)
        {
            if (location != null)
                return false;

            if(bobberTile != Vector2.Zero)
            {
                foreach (Building b in __instance.buildings)
                {
                    if (b is FishPond && b.isTileFishable(bobberTile))
                    {
                        __result = (b as FishPond).CatchFish();
                        return true;
                    }
                }
            }

            SwapFishingPool(ref bobberTile, ref location);
            return false;
        }

        [HarmonyPatch(typeof(GameLocation), "getFishingLocation")]
        [HarmonyPrefix]
        public static bool GetFishingLocationPatch(ref Vector2 tile, ref int __result)
        {
            foreach((var region, int id) in idRegions.Value)
            {
                if (region.Contains(tile))
                {
                    __result = id;
                    return true;
                }
            }
            return false;
        }
    }
}
