using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewModdingAPI.Utilities;
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

            if (!int.TryParse(split[0], out int count) || count <= 0 || !int.TryParse(split[1], out int id) || id < 0)
                return;

            if (!Game1.objectInformation.TryGetValue(id, out string info))
                return;

            string name = info.Split('/')[0];
            object templ = new{ what = split[0] + "x " + name};
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
            Game1.addMail(split[2], true, true);

            if (!who.IsLocalPlayer)
                return;

            Dictionary<string, string> events;
            string ev = null;
            
            if(split.Length <= 3)
            {
                try
                {
                    events = who.currentLocation.GetLocationEvents();
                    ev = events[split[3]];
                }
                catch (Exception)
                {
                }
            }
            string path = who.currentLocation.mapPath;
            Vector2 coords = who.getTileLocation();
            string name = who.currentLocation.Name;

            Events.afterFadeQueue.Add(() =>
            {
                if (ev != null)
                {
                    who.currentLocation.startEvent(new(ev)
                    {
                        onEventFinished = () =>
                        {
                            ReloadCurrentLocation(who, path, coords, name);
                        }
                    });
                } else
                {
                    ReloadCurrentLocation(who, path, coords, name);
                }
            });
            Game1.fadeScreenToBlack();
        }
        public static void ReloadCurrentLocation(Farmer who, string path, Vector2 coords, string name)
        {
            Events.drawVoid = true;
            ModEntry.helper.Content.InvalidateCache(path);
            if(who.currentLocation.mapPath == path)
                Utils.warpToTempMap("EventVoid", who);
            Events.afterFadeQueue.Add(() =>
            {
                Events.drawVoid = false;
            });
            who.warpFarmer(new(0, 0, name, (int)coords.X, (int)coords.Y, false));
        }
        public static Response[] MakeResponses(string val)
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            responses[0].responseKey = val;
            return responses;
        }
    }
}
