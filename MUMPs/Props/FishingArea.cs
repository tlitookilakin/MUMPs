using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Props
{
    class FishingArea
    {
        public static bool GetFishingLocationPatch(ref Vector2 tile, GameLocation __instance, ref int __result)
        {
            string[] data = Utils.MapPropertyArray(__instance, "FishingAreas");
            for(int i = 0; i + 4 < data.Length; i += 5)
            {
                if( !int.TryParse(data[i], out int ind))
                {
                    return false;
                }
                Vector2 origin = Utils.StringsToVec2(data[i + 1], data[i + 2]);
                Vector2 size = Utils.StringsToVec2(data[i + 3], data[i + 4]);
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
