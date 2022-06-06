using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
namespace MUMPs.Props
{
    [ModInit]
    class ActionWarpList
    {
        internal static void Init()
        {
            ModEntry.AeroAPI.RegisterAction("WarpList", display);
        }
        private static void display(Farmer who, string action, Point _)
        {
            var split = action.SafeSplit(' ');
            List<Response> opts = new();
            for(int i = 0; i + 3 < split.Count; i += 4)
                opts.Add(new($"'{split[i + 1]}' '{split[i + 2]}' '{split[i + 3]}'", split[i]));
            Misc.ShowPagedResponses(ModEntry.i18n.Get("warpmenu.title"), opts.ToArray(), selected);
        }
        private static void selected(Farmer who, string target)
        {
            var split = target.SafeSplit(' ');
            if (Game1.getLocationFromName(split[2]) == null)
                return;
            if (!int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            Game1.warpFarmer(split[2], x, y, 0);
        }
    }
}
