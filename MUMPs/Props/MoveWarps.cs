using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Props
{
    class MoveWarps
    {
        public static void CorrectWarp(WarpedEventArgs ev)
        {
            string[] warps = Utils.MapPropertyArray(ev.NewLocation,"MoveWarps");
            Point pos = ev.Player.getTileLocationPoint();
            for(int i = 0; i + 3 < warps.Length; i += 4)
            {
                if(warps.StringsToPoint(out Point point, i) && pos == point)
                {
                    if(warps.StringsToVec2(out Vector2 to, i + 2))
                    {
                        ev.Player.setTileLocation(to);
                        ModEntry.monitor.Log("Redirected player from " + point.ToString() + " to " + to.ToString() + ".", LogLevel.Trace);
                        return;
                    } else
                    {
                        ModEntry.monitor.Log("Could not read MoveWarps property @ " + ev.NewLocation.Name + ", invalid format.", LogLevel.Warn);
                        return;
                    }
                }
            }
        }
    }
}
