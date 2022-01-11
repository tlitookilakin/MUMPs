﻿using HarmonyLib;
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
        private static readonly PerScreen<List<RepairSpot>> currentSet = new(() => new());
        public static void Draw(SpriteBatch b)
        {
            foreach(var spot in currentSet.Value)
            {
                spot.Draw(b);
            }
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentSet.Value.Clear();
            var map = loc.map;
            if (map == null || loc.Name == "Temp")
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
                        currentSet.Value.Add(new(x, y));
                    }
                }
            }
        }
        public static void DoAction(Farmer who, string action)
        {
            if (who.currentLocation.name == "Temp")
                return;

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

            MessageRepairEvent msg = new(who.currentLocation.Name, (split.Length > 3) ? split[3] : null);
            EventAndReload(msg);
            ModEntry.helper.Multiplayer.SendMessage(msg, "RepairEvent", new string[]{ModEntry.ModID});
        }
        public static void EventAndReload(MessageRepairEvent msg)
        {
            GameLocation loc = Game1.getLocationFromName(msg.LocationName);

            if (Game1.currentLocation != loc)
                return;

            string ev = null;

            if (msg.EventName != null)
            {
                try
                {
                    ev = loc.GetLocationEvents()[msg.EventName];
                }
                catch (Exception)
                {
                }
            }
            string path = loc.mapPath;
            Vector2 coords = Game1.player.getTileLocation();

            Events.afterFadeQueue.Add(() =>
            {
                if (ev != null)
                {
                    Game1.currentLocation.startEvent(new(ev)
                    {
                        onEventFinished = () =>
                        {
                            ReloadCurrentLocation(path, coords, msg.LocationName);
                        }
                    });
                }
                else
                {
                    ReloadCurrentLocation(path, coords, msg.LocationName);
                }
            });
            Game1.fadeScreenToBlack();
        }
        public static void ReloadCurrentLocation(string path, Vector2 coords, string name)
        {
            Events.drawVoid = true;
            ModEntry.helper.Content.InvalidateCache(path);
            if(Game1.currentLocation.mapPath == path)
                Utils.warpToTempMap("EventVoid", Game1.player);
            Events.afterFadeQueue.Add(() =>
            {
                Events.drawVoid = false;
            });
            Game1.warpFarmer(name, (int)coords.X, (int)coords.Y, false);
        }
        public static Response[] MakeResponses(string val)
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            responses[0].responseKey = val;
            return responses;
        }
    }
}
