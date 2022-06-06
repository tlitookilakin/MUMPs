using AeroCore;
using AeroCore.API;
using AeroCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs
{
    [ModInit]
    class Events
    {
        public static readonly PerScreen<List<Action>> afterFadeQueue = new(() => new());
        private static readonly PerScreen<bool> wasFading = new(() => false);
        private static readonly PerScreen<bool> fadeWasRun = new(() => false);
        public static readonly PerScreen<bool> drawVoid = new(() => false);
        internal static readonly PerScreen<bool> reloadScreen = new(() => false);
        public static void DayStarted(object sender, DayStartedEventArgs ev)
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
        public static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayStarted += DayStarted;
            ModEntry.helper.Events.Player.Warped += ChangeLocation;
            ModEntry.helper.Events.GameLoop.UpdateTicked += Tick;
            ModEntry.helper.Events.GameLoop.SaveLoaded += EnterWorld;
            ModEntry.helper.Events.Display.RenderedHud += DrawOverHud;
            ModEntry.helper.Events.Multiplayer.ModMessageReceived += RecieveMessage;
        }
        public static void EnterWorld(object sender, SaveLoadedEventArgs ev)
        {
            Props.LightingLayer.Reload();
            EnterLocation(Game1.currentLocation);
        }
        public static void DrawOverHud(object sender, RenderedHudEventArgs ev)
        {
            DrawVoid(ev.SpriteBatch);
        }
        public static void Tick(object sender, UpdateTickedEventArgs ev)
        {
            if (reloadScreen.Value)
            {
                reloadScreen.Value = false;
                EnterLocation(Game1.currentLocation);
            }
            RunFade();
        }
        public static void ChangeLocation(object sender, WarpedEventArgs ev)
        {
            EnterLocation(ev.NewLocation);
        }
        public static void EnterLocation(GameLocation location)
        {
            drawVoid.Value = location.mapPath.Value == PathUtilities.NormalizeAssetName("Maps/EventVoid");
        }
        public static void RunFade()
        {
            if (wasFading.Value != Game1.IsFading())
            {
                fadeWasRun.Value = false;
            }
            if (!Game1.fadeIn && wasFading.Value && !fadeWasRun.Value)
            {
                var existingQueue = afterFadeQueue.Value.ToArray();
                afterFadeQueue.Value.Clear();
                foreach (var action in existingQueue)
                {
                    action();
                }
                fadeWasRun.Value = true;
            }
            wasFading.Value = Game1.IsFading();
        }
        public static void RecieveMessage(object sender, ModMessageReceivedEventArgs ev)
        {
            if (ev.FromModID != ModEntry.ModID)
                return;

            switch (ev.Type)
            {
                case "RepairEvent":
                    Props.ActionRepair.EventAndReload(ev.ReadAs<models.MessageRepairEvent>());
                    break;
                case "ReloadEvent":
                    Utility.ReceiveReloadRequest(ev.ReadAs<models.MessageRepairEvent>());
                    break;
                default:
                    ModEntry.monitor.Log("Unhandled message type: " + ev.Type, LogLevel.Warn);
                    break;
            }
        }
        public static void DrawVoid(SpriteBatch b)
        {
            if (drawVoid.Value)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black);
            }
        }
    }
}
