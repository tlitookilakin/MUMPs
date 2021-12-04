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
                if(Utils.StringsToPoint(warps[i], warps[i + 1]) == pos){
                    ev.Player.setTileLocation(Utils.StringsToVec2(warps[i + 2], warps[i + 3]));
                    ModEntry.monitor.Log("Redirected player from [" + warps[i] + ", " + warps[i + 1] + "] to [" + warps[i + 2] + ", " + warps[i + 3] + "].", LogLevel.Trace);
                    break;
                }
            }
        }
    }
}
