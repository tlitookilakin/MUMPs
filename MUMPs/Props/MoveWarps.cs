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
        public static void CorrectWarp(object sender, WarpedEventArgs ev)
        {
            string[] warps = ev.NewLocation.getMapProperty("MoveWarps").Split(' ');
            Point pos = ev.Player.getTileLocationPoint();
            for(int i = 0; i + 3 < warps.Length; i += 4)
            {
                if(int.Parse(warps[i]) == pos.X && int.Parse(warps[i + 1]) == pos.Y){
                    ev.Player.setTileLocation(new Vector2(float.Parse(warps[i + 2]), float.Parse(warps[i + 3])));
                    ModEntry.monitor.Log("Redirected player from [" + warps[i] + ", " + warps[i + 1] + "] to [" + warps[i + 2] + ", " + warps[i + 3] + "].", LogLevel.Trace);
                    break;
                }
            }
        }
    }
}
