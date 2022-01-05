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
            EnterLocation(Game1.currentLocation);
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
            EnterLocation(ev.NewLocation);
        }
        public static void EnterLocation(GameLocation location)
        {
            Props.Birds.EnterLocation(location);
            Props.Horizon.ChangeLocation(location);
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
