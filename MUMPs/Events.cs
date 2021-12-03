using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs
{
    class Events
    {
        public static void DayStarted(object sender, DayStartedEventArgs ev)
        {
            foreach(GameLocation loc in Game1.locations)
            {
                if (loc.Name != "Woods")
                {
                    Props.Stumps.SpawnMapStumps(loc);
                }
            }
        }
    }
}
