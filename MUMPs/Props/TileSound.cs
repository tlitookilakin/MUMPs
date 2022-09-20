using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
	[ModInit]
	internal class TileSound
	{
		private static readonly PerScreen<Dictionary<ICue, List<Vector2>>> soundSources = new(() => new());
		private static readonly PerScreen<float> fadeVolume = new(() => -.5f);

		internal static void Init()
		{
			ModEntry.OnChangeLocation += PopulateSounds;
			ModEntry.OnCleanup += Cleanup;
			ModEntry.OnTick += Tick;
			ModEntry.AeroAPI.LocationCleanup += LocationCleanup;
		}

		private static void PopulateSounds(GameLocation where)
		{
			var data = new Dictionary<ICue, List<Vector2>>();
			var soundCache = new Dictionary<string, ICue>();
			foreach ((var tile, var x, var y) in where.Map.TilesInLayer("Paths"))
			{
				if (!tile.TileHasProperty("Sound", out var s))
					continue;
				s = s.Trim();
				ICue cue = soundCache.TryGetValue(s, out var c) ? c : soundCache[s] = Game1.soundBank.GetCue(s);
				if (cue is null)
					continue;
				var points = data.TryGetValue(c, out var p) ? p : data[c] = new();
				points.Add(new(x, y));
				cue.Volume = 0f;
				cue.Play();
			}
			soundSources.Value = data;
		}
		private static void CheckSplitScreenStop(object _, PeerDisconnectedEventArgs ev)
		{
			if (ev.Peer.IsSplitScreen)
				StopAll(soundSources.GetValueForScreen((int)ev.Peer.ScreenID));
		}
		private static void LocationCleanup(GameLocation where)
		{
			StopAll();
			fadeVolume.Value = -.5f;
		}
		private static void StopAll(Dictionary<ICue, List<Vector2>> which = null)
		{
			which ??= soundSources.Value;
			foreach (var cue in which.Keys)
			{
				cue.Stop(AudioStopOptions.Immediate);
				cue.Dispose();
			}
		}
		private static void Cleanup()
		{

			fadeVolume.ResetAllScreens();
			foreach ((_, var v) in soundSources.GetActiveValues())
				StopAll(v);
			soundSources.ResetAllScreens();
		}
		private static void Tick()
		{
			if (fadeVolume.Value < 1f)
				fadeVolume.Value += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds * .0003f;

			var vol = Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel);
			var pos = Game1.player.Position;
			foreach((var cue, var points) in soundSources.Value)
			{
				float nearest = float.PositiveInfinity;
				foreach (var point in points)
					nearest = MathF.Min(nearest, Vector2.Distance(point, pos));
				if (nearest > 1024){
					cue.Pause(); continue;
				}
				nearest = MathF.Min(1f - nearest / 1024, fadeVolume.Value);
				cue.Volume =  nearest * 100f * vol;
				cue.Resume();
			}
		}
	}
}
