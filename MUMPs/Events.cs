using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs
{
    class Events
    {
        public static readonly PerScreen<List<Action>> afterFadeQueue = new(() => new());
        private static readonly PerScreen<bool> wasFading = new(() => false);
        private static readonly PerScreen<bool> fadeWasRun = new(() => false);
        public static readonly PerScreen<bool> drawVoid = new(() => false);
        public static void DayStarted(object sender, DayStartedEventArgs ev)
        {
            if (!Game1.IsClient)
            {
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc.Name != "Woods")
                    {
                        Props.Stumps.SpawnMapStumps(loc);
                    }
                }
            }
        }
        public static void EnterWorld(object sender, SaveLoadedEventArgs ev)
        {
            Props.LightingLayer.Reload();
            EnterLocation(Game1.currentLocation);
        }
        public static void DrawOnTop(object sender, RenderedWorldEventArgs ev)
        {
            Props.Birds.DrawAbove(ev.SpriteBatch);
            Props.ActionRepair.Draw(ev.SpriteBatch);
            Props.Horizon.DrawAfter(ev.SpriteBatch);
        }
        public static void DrawOverHud(object sender, RenderedHudEventArgs ev)
        {
            DrawVoid(ev.SpriteBatch);
        }
        public static void Tick(object sender, UpdateTickedEventArgs ev)
        {
            RunFade();
            Props.Birds.Update(Game1.currentGameTime);
        }
        public static void ChangeLocation(object sender, WarpedEventArgs ev)
        {
            Props.MoveWarps.CorrectWarp(ev);
            EnterLocation(ev.NewLocation);
        }
        public static void EnterLocation(GameLocation location)
        {
            drawVoid.Value = (location.mapPath == PathUtilities.NormalizeAssetName("Maps/EventVoid"));
            Props.Birds.EnterLocation(location);
            Props.Horizon.ChangeLocation(location);
            Props.ActionRepair.ChangeLocation(location);
            Props.Butterflies.EnterLocation(location);
            Props.CamRegion.ChangeLocation(location);
        }
        public static void OnQuit(object sender, ReturnedToTitleEventArgs ev)
        {
            Cleanup();
        }
        public static void Cleanup()
        {
            Props.Birds.Cleanup();
            Props.Horizon.Cleanup();
            Props.CamRegion.Cleanup();
            Props.ActionRepair.Cleanup();
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
