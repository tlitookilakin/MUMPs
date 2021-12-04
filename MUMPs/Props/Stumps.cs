using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace MUMPs.Props
{
    class Stumps
    {
        internal static void SpawnMapStumps(GameLocation location)
        {
            string[] stumpList = Utils.MapPropertyArray(location, "Stumps");
            if(stumpList.Length > 0)
            {
                ModEntry.monitor.Log("Adding stumps to " + location.Name + ".", LogLevel.Trace);
            }
            for(int i = 0; i+2 < stumpList.Length; i += 3)
            {
                //x, y, unused

                Point pos = Utils.StringsToPoint(stumpList[i], stumpList[i + 1]);
                if (location.isAreaClear(new Rectangle(pos, new Point(2, 2))))
                {
                    //will not be saved in most locations, but that's fine because they are regenerated at day start anyways
                    location.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, pos.ToVector2());
                }
            }
        }
    }
}
