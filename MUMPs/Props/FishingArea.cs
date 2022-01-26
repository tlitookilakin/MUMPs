﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class FishingArea
    {
        [HarmonyPatch(typeof(GameLocation), "getFishingLocation")]
        [HarmonyPrefix]
        public static bool GetFishingLocationPatch(ref Vector2 tile, GameLocation __instance, ref int __result)
        {
            string[] data = Utils.MapPropertyArray(__instance, "FishingRegions");
            for(int i = 0; i + 4 < data.Length; i += 5)
            {
                if( !int.TryParse(data[i + 4], out int ind))
                {
                    return false;
                }
                Vector2 origin = Utils.StringsToVec2(data[i], data[i + 1]);
                Vector2 size = Utils.StringsToVec2(data[i + 2], data[i + 3]);
                if (tile.X >= origin.X && tile.Y >= origin.Y && tile.X < origin.X + size.X && tile.Y < origin.Y + size.Y)
                {
                    __result = ind;
                    return true;
                }
            }
            return false;
        }
    }
}
