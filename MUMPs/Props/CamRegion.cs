using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class CamRegion
    {
        private static readonly PerScreen<List<Rectangle>> regions = new(() => new());
        public static void ChangeLocation(GameLocation loc)
        {
            regions.Value.Clear();
            string[] split = loc.getMapProperty("CamRegions")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (split == null)
                return;

            for(int i = 0; i + 3 < split.Length; i += 4)
            {
                if(!int.TryParse(split[i], out int x) || !int.TryParse(split[i + 1], out int y) || !int.TryParse(split[i + 2], out int w) || !int.TryParse(split[i + 3], out int h))
                {
                    ModEntry.monitor.Log("Failed to parse CamRegion map property @ " + loc.Name + ": could not convert to number.", LogLevel.Warn);
                    regions.Value.Clear();
                    return;
                }
                regions.Value.Add(new(x * 64, y * 64, w * 64, h * 64));
            }
        }
        public static void Cleanup()
        {
            regions.ResetAllScreens();
        }
        [HarmonyPatch(typeof(Game1), "UpdateViewPort")]
        [HarmonyPrefix]
        public static bool UpdateCamera(bool overrideFreeze, Point centerPoint)
        {
            if (Game1.currentLocation.forceViewportPlayerFollow || (!overrideFreeze && Game1.viewportFreeze))
                return true;

            foreach(var region in regions.Value)
            {
                if (region.Contains(centerPoint))
                {
                    centerPoint.X = (Game1.viewport.Width >= region.Width) ? region.X + region.Width / 2 :
                        Math.Clamp(centerPoint.X, region.X + Game1.viewport.Width / 2, region.X + region.Width - Game1.viewport.Width / 2);
                    centerPoint.Y = (Game1.viewport.Height >= region.Height) ? region.Y + region.Height / 2 :
                        Math.Clamp(centerPoint.Y, region.Y + Game1.viewport.Height / 2, region.Y + region.Height - Game1.viewport.Height / 2);
                }
            }
            return true;
        }
    }
}
