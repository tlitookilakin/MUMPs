using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public static readonly List<Action> afterFadeQueue = new();
        private static bool wasFading = false;
        private static bool fadeWasRun = false;
        public static bool drawVoid = false;
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
            drawVoid = (location.mapPath == PathUtilities.NormalizeAssetName("Maps/EventVoid"));
            Props.Birds.EnterLocation(location);
            Props.Horizon.ChangeLocation(location);
            Props.ActionRepair.ChangeLocation(location);
            Props.Butterflies.EnterLocation(location);
        }
        public static void OnQuit(object sender, ReturnedToTitleEventArgs ev)
        {
            Cleanup();
        }
        public static void Cleanup()
        {
            Props.Birds.Cleanup();
            Props.Horizon.Cleanup();
        }
        public static void RunFade()
        {
            if (wasFading != Game1.IsFading())
            {
                fadeWasRun = false;
            }
            if (!Game1.fadeIn && wasFading && !fadeWasRun)
            {
                var existingQueue = afterFadeQueue.ToArray();
                afterFadeQueue.Clear();
                foreach (var action in existingQueue)
                {
                    action();
                }
                fadeWasRun = true;
            }
            wasFading = Game1.IsFading();
        }
        public static void DrawVoid(SpriteBatch b)
        {
            if (drawVoid)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black);
            }
        }
    }
}
