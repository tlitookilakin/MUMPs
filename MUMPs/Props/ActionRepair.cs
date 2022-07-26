using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [ModInit]
    class ActionRepair
    {
        private static readonly PerScreen<List<RepairSpot>> currentSet = new(() => new());
        internal static void Init()
        {
            ModEntry.AeroAPI.RegisterAction("Repair", DoAction, 6);
            ModEntry.OnDraw += Draw;
            ModEntry.OnCleanup += Cleanup;
            ModEntry.OnChangeLocation += ChangeLocation;
        }
        private static void Draw(SpriteBatch b)
        {
            foreach(var spot in currentSet.Value)
                spot.Draw(b);
        }
        private static void ChangeLocation(GameLocation loc)
        {
            currentSet.Value.Clear();
            var map = loc.map;
            if (map == null || loc.Name == "Temp")
                return;
            var buildings = map.GetLayer("Buildings");

            foreach((var tile, int x, int y) in Maps.TilesInLayer(buildings))
            {
                if (!tile.Properties.TryGetValue("Action", out var action) && !tile.TileIndexProperties.TryGetValue("Action", out action))
                    continue;

                if (action.ToString().Trim().StartsWith("Repair"))
                    currentSet.Value.Add(new(x, y));
            }
        }
        private static void Cleanup() => currentSet.ResetAllScreens();
        private static void DoAction(Farmer who, string action, Point _, GameLocation where)
        {

            if (where.Name == "Temp")
                return;

            var split = action.SafeSplit(' ');
            if (split.Count < 3)
                return;

            if (!int.TryParse(split[0], out int count) || count <= 0 || !split[1].TryGetItem(out Item what))
                return;

            object templ = new{ what = $"{split[0]}x {what.DisplayName}"};

            // uses name and not direct match in case multiple items have the same name
            // and it confuses the player

            if (!who.HasItemNamed(what.Name, count))
                Game1.drawObjectDialogue(Game1.parseText(ModEntry.i18n.Get("repair.need",templ)));
            else
                who.currentLocation.createQuestionDialogue(
                    Game1.parseText(ModEntry.i18n.Get("repair.use", templ)),
                    MakeResponses(action),AnswerYesNo);
        }
        private static void AnswerYesNo(Farmer who, string answer)
        {
            if (answer.ToLower() == "no")
                return;

            var split = answer.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2 || !int.TryParse(split[0], out int count) || count < 1 || !split[1].TryGetItem(out Item what))
                return;

            what.Stack = count;
            who.RemoveNamedItemsFromInventory(what.Name, count);
            Game1.addMail(split[2], true, true);

            MessageRepairEvent msg = new(who.currentLocation.Name, (split.Length > 3) ? split[3] : null);
            EventAndReload(msg);
            ModEntry.helper.Multiplayer.SendMessage(msg, "RepairEvent", new string[]{ModEntry.ModID});
        }
        internal static void EventAndReload(MessageRepairEvent msg)
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
            string path = loc.mapPath.Value;
            Vector2 coords = Game1.player.getTileLocation();

            Events.afterFadeQueue.Value.Add(() =>
            {
                if (ev != null)
                {
                    Game1.currentLocation.startEvent(new(ev)
                    {
                        onEventFinished = () =>
                        {
                            Utility.ReloadCurrentLocation(path, coords, msg.LocationName);
                            if (Context.IsMainPlayer)
                                Patches.BlockedTileClearer.ClearBlockedTilesIn(loc);
                        }
                    });
                }
                else
                {
                    Utility.ReloadCurrentLocation(path, coords, msg.LocationName);
                    if (Context.IsMainPlayer)
                        Patches.BlockedTileClearer.ClearBlockedTilesIn(loc);
                }
            });
            Game1.fadeScreenToBlack();
        }
        private static Response[] MakeResponses(string val)
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            responses[0].responseKey = val;
            return responses;
        }
    }
}
