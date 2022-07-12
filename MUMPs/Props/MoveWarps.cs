using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;

namespace MUMPs.Props
{
    [ModInit]
    class MoveWarps
    {
        internal static void Init()
        {
            ModEntry.helper.Events.Player.Warped += CorrectWarp;
        }
        private static void CorrectWarp(object _, WarpedEventArgs ev)
        {
            string[] warps = Maps.MapPropertyArray(ev.NewLocation,"MoveWarps");
            Point pos = ev.Player.getTileLocationPoint();
            for(int i = 0; i + 3 < warps.Length; i += 4)
            {
                if(warps.ToPoint(out Point point, i) && pos == point)
                {
                    if(warps.ToVector2(out Vector2 to, i + 2))
                    {
                        ev.Player.setTileLocation(to);
                        ModEntry.monitor.Log($"Redirected player from {point} to {to}.", LogLevel.Trace);
                        return;
                    } else
                    {
                        ModEntry.monitor.Log($"Could not read MoveWarps property @ {ev.NewLocation.Name}, invalid format.", LogLevel.Warn);
                        return;
                    }
                }
            }
        }
    }
}
