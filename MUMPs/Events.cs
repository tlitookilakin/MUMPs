using StardewModdingAPI.Events;
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
            Props.Birds.EnterLocation(Game1.currentLocation);
            Props.Horizon.ChangeLocation(Game1.currentLocation);
        }
        public static void DrawOnBottom(object sender, RenderingWorldEventArgs ev)
        {
            Props.Horizon.DrawBefore(ev.SpriteBatch);
        }
        public static void DrawOnTop(object sender, RenderedWorldEventArgs ev)
        {
            Props.Birds.DrawAbove(ev.SpriteBatch);
            Props.Horizon.DrawAfter(ev.SpriteBatch);
        }
        public static void Tick(object sender, UpdateTickedEventArgs ev)
        {
            Props.Birds.Update(Game1.currentGameTime);
        }
        public static void ChangeLocation(object sender, WarpedEventArgs ev)
        {
            Props.MoveWarps.CorrectWarp(ev);
            Props.Birds.EnterLocation(ev.NewLocation);
            Props.Horizon.ChangeLocation(ev.NewLocation);
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
    }
}
