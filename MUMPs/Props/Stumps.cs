using Microsoft.Xna.Framework;
using StardewValley;

namespace MUMPs.Props
{
    class Stumps
    {
        internal static void SpawnMapStumps(GameLocation location)
        {
            string prop = location.getMapProperty("Stumps");
            if (prop == null) { return; }
            string[] stumpList = prop.Split(' ');
            for(int i = 0; i+2 < stumpList.Length; i += 3)
            {
                //x, y, unused

                Point pos = new Point(int.Parse(stumpList[i]), int.Parse(stumpList[i + 1]));
                if (location.isAreaClear(new Rectangle(pos, new Point(2, 2))))
                {
                    location.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, pos.ToVector2());
                }
            }
        }
    }
}
