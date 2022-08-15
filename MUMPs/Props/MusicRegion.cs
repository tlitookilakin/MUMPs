using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [ModInit]
    class MusicRegion
    {
        public static readonly Dictionary<Rectangle, string> regions = new();
        private static string lastCue = "";
        private static Point lastPos = default;

        internal static void Init()
        {
            ModEntry.OnChangeLocation += ChangeLocation;
            ModEntry.OnCleanup += Cleanup;
            ModEntry.OnTick += () => Update(false);
        }
        private static void ChangeLocation(GameLocation loc)
        {
            if (!Context.IsMainPlayer)
                return;

            regions.Clear();
            if (!UpdateRegions(loc))
            {
                if (lastCue != "")
                    Game1.stopMusicTrack(Game1.MusicContext.SubLocation);
                lastCue = "";
            } else
            {
                Update(true);
            }
        }
        private static bool UpdateRegions(GameLocation loc)
        {
            string[] data = loc.getMapProperty("MusicRegions")?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (data == null)
                return false;

            for(int i = 0; i + 4 < data.Length; i += 5)
            {
                if(data.ToRect(out Rectangle rect, i))
                {
                    regions[rect] = data[i + 4];
                } else
                {
                    ModEntry.monitor.Log("Invalid MusicRegions property value on the map for " + loc.Name, LogLevel.Warn);
                    regions.Clear();
                    return false;
                }
            }
            return true;
        }
        private static void Cleanup()
        {
            regions.Clear();
            lastCue = "";
            Game1.stopMusicTrack(Game1.MusicContext.SubLocation);
        }
        private static void Update(bool force)
        {
            var who = Game1.player;
            if (who.currentLocation == null || Context.IsSplitScreen && !Context.IsMainPlayer)
                return;

            Point pos = who.getTileLocationPoint();
            if (!force && pos == lastPos)
            {
                if (lastCue != "" && Game1.isMusicContextActiveButNotPlaying(Game1.MusicContext.SubLocation))
                    Game1.changeMusicTrack(lastCue, true, Game1.MusicContext.SubLocation);
                return;
            }
            lastPos = pos;

            string cue = who.currentLocation.getMapProperty("Music") ?? "";
            foreach((var region, string song) in regions)
            {
                if (region.Contains(pos))
                {
                    cue = song;
                    break;
                }
            }
            if (cue == lastCue && !Game1.isMusicContextActiveButNotPlaying(Game1.MusicContext.SubLocation))
                return;

            lastCue = cue;
            if (cue == "")
                Game1.stopMusicTrack(Game1.MusicContext.SubLocation);
            else
                Game1.changeMusicTrack(cue, true, Game1.MusicContext.SubLocation);
        }
    }
}
