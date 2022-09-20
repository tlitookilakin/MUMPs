using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MUMPs.Props
{
    [ModInit]
    class MusicRegion
    {
        private const string contextName = "_instanceActiveMusicContext";

        public static readonly PerScreen<Dictionary<Rectangle, string>> regions = new(() => new());
        private static readonly PerScreen<string> lastCue = new(() => "");
        private static readonly PerScreen<Point> lastPos = new();
        private static readonly PerScreen<bool> locChanged = new();
        private static readonly PerScreen<Dictionary<ICue, bool>> cueIsFade = new(() => new());
        private static readonly PerScreen<List<ICue>> oldCues = new(() => new()); 
        private static readonly PerScreen<bool> ChannelFade = new();
        private static int ActiveScreen = 0;
        private static float MainGameVolume = 1f;

        private static readonly ConditionalWeakTable<Game1, IReflectedField<Game1.MusicContext>> activeContext = new();
        private static readonly ConditionalWeakTable<Game1, Dictionary<Game1.MusicContext, KeyValuePair<string, bool>>> activeTracks = new();
        private static FieldInfo trackField;

        internal static void Init()
        {
            trackField = typeof(Game1).FieldNamed("_instanceRequestedMusicTracks");

            ModEntry.OnChangeLocation += ChangeLocation;
            ModEntry.OnCleanup += Cleanup;
            ModEntry.OnTick += Update;
        }
        private static void ChangeLocation(GameLocation loc)
        {
            locChanged.Value = true;
            regions.Value.Clear();
            UpdateRegions(loc);
            Update();
            locChanged.Value = false;
        }
        private static void UpdateRegions(GameLocation loc)
        {
            var reg = regions.Value;
            reg.Clear();
            string[] data = loc.getMapProperty("MusicRegions")?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (data == null)
                return;

            for(int i = 0; i + 4 < data.Length; i += 5)
            {
                if(data.ToRect(out Rectangle rect, i))
                {
                    reg[rect] = data[i + 4];
                } else
                {
                    ModEntry.monitor.Log("Invalid MusicRegions property value on the map for " + loc.Name, LogLevel.Warn);
                    reg.Clear();
                    break;
                }
            }
            var knownCues = reg.Values.ToArray();
            List<ICue> toRemove = new();
            var cueFade = cueIsFade.Value;
            var old = oldCues.Value;
            foreach(var cue in cueFade.Keys)
            {
                if (!knownCues.Contains(cue.Name))
                {
                    if (cue.Volume > 0f)
                    {
                        cueFade[cue] = false;
                        old.Add(cue);
                    }
                    else
                    {
                        toRemove.Add(cue);
                    }
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                var cue = toRemove[i];
                cue.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                cueFade.Remove(cue);
                cue.Dispose();
            }
        }
        private static void Cleanup()
        {
            regions.Value.Clear();
            lastCue.Value = "";
        }
        private static void UpdateScreenVolume()
        {
            if (Game1.game1.IsMainInstance)
            {
                ActiveScreen = Game1.game1.instanceId;
                bool OverrideMain = false;
                foreach (var game in GameRunner.instance.gameInstances)
                {
                    if (!activeContext.TryGetValue(game, out var field))
                        activeContext.Add(game, field = ModEntry.helper.Reflection.GetField<Game1.MusicContext>(game, contextName));

                    if (!activeTracks.TryGetValue(game, out var tracks))
                        activeTracks.Add(game, tracks = (Dictionary<Game1.MusicContext, KeyValuePair<string, bool>>)trackField.GetValue(game));

                    Game1.MusicContext ctx = field.GetValue();
                    if (tracks.ContainsKey(ctx) && tracks[ctx].Key == "mermaidSong")
                        ctx = Game1.MusicContext.MAX;

                    if (ctx > Game1.MusicContext.SubLocation)
                    {
                        OverrideMain = false;
                        ActiveScreen = Game1.game1.instanceId;
                        break;
                    }
                    if (lastCue.GetValueForScreen(game.instanceId) != "" && !OverrideMain)
                    {
                        if (game.IsMainInstance)
                            OverrideMain = true;
                        ActiveScreen = game.instanceId;
                    }
                }
                ChannelFade.Value = ActiveScreen == Game1.game1.instanceId && OverrideMain;
            }
            else
            {
                ChannelFade.Value = ActiveScreen == Game1.game1.instanceId;
            }
        }
        private static void UpdateTrackVolume()
        {
            var force = ChannelFade.Value;
            foreach((var cue, var fade) in cueIsFade.Value)
            {
                if (fade && force)
                {
                    if (cue.IsPaused)
                        cue.Resume();
                    else if (!cue.IsPlaying)
                        cue.Play();
                    cue.Volume = MathF.Min(1f, cue.Volume + .01f);
                }
                else
                {
                    cue.Volume = MathF.Max(0f, cue.Volume - .01f);
                    if (cue.Volume == 0 && !cue.IsPaused)
                        cue.Pause();
                }
            }
            var old = oldCues.Value;
            for(int i = old.Count - 1; i >= 0; i--)
            {
                var n = old[i];
                if (!n.IsPlaying || n.Volume == 0)
                {
                    n.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                    cueIsFade.Value.Remove(n);
                    n.Dispose();
                    old.RemoveAt(i);
                }
            }
            if (Game1.game1.IsMainInstance)
            {
                if (force)
                    MainGameVolume = MathF.Max(0f, MainGameVolume - .01f);
                else
                    MainGameVolume = MathF.Min(1f, MainGameVolume + .01f);
                if (Game1.currentSong is not null)
                    Game1.currentSong.Volume = MainGameVolume;
            }
        }
        private static void Update()
        {
            Point pos = Game1.player.getTileLocationPoint();
            bool changed = locChanged.Value || lastPos.Value != pos;
            lastPos.Value = pos;
            if (!Context.IsWorldReady)
                return;
            UpdateScreenVolume();
            UpdateTrackVolume();

            if (Game1.currentLocation is null || !changed)
                return;

            lastCue.Value = "";
            foreach((var region, string song) in regions.Value)
            {
                if (region.Contains(pos))
                {
                    lastCue.Value = song;
                    break;
                }
            }

            bool fade = ActiveScreen == Context.ScreenId;
            bool foundCue = false;
            var isFade = cueIsFade.Value;
            var id = lastCue.Value;
            foreach (var cue in isFade.Keys)
            {
                if (cue.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    foundCue = true;
                    isFade[cue] = fade;
                }
                else
                {
                    isFade[cue] = false;
                }
            }
            if (!foundCue && id != "")
            {
                var cue = Game1.soundBank.GetCue(id);
                isFade.Add(cue, fade);
            }
        }
    }
}
