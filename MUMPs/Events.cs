using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace MUMPs
{
    [ModInit]
    class Events
    {
        internal static readonly PerScreen<List<Action>> afterWarpActions = new(() => new());
        internal static readonly PerScreen<bool> reloadScreen = new(() => false);
        private static IReflectedField<ScreenFade> gameFade;
        private static void DayStarted(object sender, DayStartedEventArgs ev)
        {
            if (!Context.IsMainPlayer)
                return;

            if (Context.IsOnHostComputer)
            {
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc.Name != "Woods")
                        Props.Stumps.SpawnMapStumps(loc);
                    Patches.BlockedTileClearer.ClearBlockedTilesIn(loc);
                }
            }
        }
        internal static void Init()
        {
            gameFade = ModEntry.helper.Reflection.GetField<ScreenFade>(typeof(Game1), "screenFade", true);
            ModEntry.helper.Events.Display.RenderedHud += 
                (s, e) => DrawVoid(e.SpriteBatch);
            ModEntry.helper.Events.Multiplayer.ModMessageReceived += RecieveMessage;
            ModEntry.helper.Events.GameLoop.DayStarted += DayStarted;
            ModEntry.helper.Events.Player.Warped += AfterWarp;
        }
        private static void AfterWarp(object _, WarpedEventArgs ev)
        {
            foreach (var action in afterWarpActions.Value)
                action?.Invoke();
            afterWarpActions.Value.Clear();
        }
        private static void RecieveMessage(object sender, ModMessageReceivedEventArgs ev)
        {
            if (ev.FromModID != ModEntry.ModID)
                return;

            switch (ev.Type)
            {
                case "ReloadEvent":
                    ReceiveReloadRequest(ev.ReadAs<string>()); break;
                default:
                    ModEntry.monitor.Log("Unhandled message type: " + ev.Type, LogLevel.Warn); break;
            }
        }
        private static void DrawVoid(SpriteBatch b)
        {
            if (Game1.currentLocation.mapPath.Value.Contains("EventVoid"))
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black);
        }
        internal static void BroadcastReloadRequest(string name)
        {
            ReceiveReloadRequest(name);
            ModEntry.helper.Multiplayer.SendMessage(name, "RepairEvent", new string[] { ModEntry.ModID });
        }
        internal static void ReceiveReloadRequest(string name)
        {
            Props.ActionRepair.EventAndReload(name);
        }
    }
}
