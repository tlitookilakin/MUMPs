﻿using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Props
{
    class ActionWarpList
    {
        public static void display(Farmer who, string action)
        {
            var split = Utils.SafeSplitList(action, ' ');
            List<Response> opts = new();
            for(int i = 0; i + 3 < split.Count; i += 4)
            {
                opts.Add(new(split[i + 1] + ' ' + split[i + 2] + ' ' + split[i + 3], split[i]));
            }
            who.currentLocation.createQuestionDialogue(ModEntry.helper.Translation.Get("warpmenu.title"), opts.ToArray(), selected);
        }
        public static void selected(Farmer who, string target)
        {
            var split = Utils.SafeSplitList(target, ' ');
            if (Game1.getLocationFromName(split[2]) == null)
                return;
            if (!int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            Game1.warpFarmer(split[2], x, y, 0);
        }
    }
}
