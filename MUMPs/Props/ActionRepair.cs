using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    class ActionRepair
    {
        private static List<RepairSpot> currentSet = new();
        public static void Draw(SpriteBatch b)
        {
            foreach(var spot in currentSet)
            {
                spot.Draw(b);
            }
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentSet.Clear();
            var map = loc.map;
            if (map == null)
                return;
            var buildings = map.GetLayer("Buildings");
            for(int x = 0; x < buildings.LayerWidth; x++)
            {
                for(int y = 0; y < buildings.LayerHeight; y++)
                {
                    var tile = buildings.Tiles[new(x, y)];
                    if (tile == null)
                        continue;
                    if (!tile.Properties.ContainsKey("Action"))
                        continue;
                    var action = tile.Properties["Action"].ToString().Trim();
                    if (action.StartsWith("Repair"))
                    {
                        currentSet.Add(new(x, y));
                    }
                }
            }
        }
        public static void DoAction(Farmer who, string action)
        {
            var split = Utils.SafeSplitList(action, ' ');
            if (split.Count < 3)
                return;

            if (!int.TryParse(split[0], out int count) || count <= 0)
                return;

            if (!int.TryParse(split[1], out int id))
                return;

            if (!Game1.bigCraftablesInformation.TryGetValue(id, out var info))
                return;

            object templ = new{ what = split[0] + "x " + info[0]};
            if (!who.hasItemInInventory(id, count))
            {
                Game1.drawObjectDialogue(Game1.parseText(ModEntry.helper.Translation.Get("repair.need",templ)));
                return;
            }
            who.currentLocation.createQuestionDialogue(Game1.parseText(ModEntry.helper.Translation.Get("repair.use", templ)),MakeResponses(action),AnswerYesNo);
        }
        public static void AnswerYesNo(Farmer who, string answer)
        {
            if (answer.ToLower() == "no")
                return;

            var split = answer.Split(' ');

            if (!int.TryParse(split[0], out int count) || !int.TryParse(split[1], out int id) || count < 1 || id < 0)
                return;

            who.removeItemsFromInventory(id, count);
            Dictionary<string, string> events;
            try
            {
                events = who.currentLocation.GetLocationEvents();
            } catch(Exception)
            {
                return;
            }
            var ev = events[split[2]];
            if (ev == null)
                return;
            who.currentLocation.startEvent(new(ev));
        }
        public static Response[] MakeResponses(string val)
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            responses[0].responseKey = val;
            return responses;
        }
    }
}
