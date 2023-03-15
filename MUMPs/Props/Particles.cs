using AeroCore;
using AeroCore.Particles;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
	[ModInit]
	internal class Particles
	{
		const float tileDepth = .0064f;

		private static readonly Rectangle defaultRect = new(0, 0, 16, 16);
		private static readonly PerScreen<List<IParticleManager>> bottomParticles = new(() => new());
		private static readonly PerScreen<List<IParticleManager>> topParticles = new(() => new());
		internal static void Init()
		{
			ModEntry.OnChangeLocation += ChangeLocation;
			ModEntry.OnCleanup += Cleanup;
			ModEntry.OnTick += Tick;
			ModEntry.AeroAPI.OnDrawingWorld += Draw;
			ModEntry.OnDraw += DrawAfter;
			//ModEntry.helper.Events.Display.RenderedHud += (s, e) => Draw(e.SpriteBatch);
		}
		private static void Cleanup()
		{
			topParticles.ResetAllScreens();
			bottomParticles.ResetAllScreens();
		}
		private static void Draw(SpriteBatch b)
		{
			Vector2 offset = new(-Game1.viewport.X, -Game1.viewport.Y);
			foreach(var particle in bottomParticles.Value)
			{
				particle.Offset = offset;
				particle.Draw(b);
			}
		}
		private static void DrawAfter(SpriteBatch b)
		{
			Vector2 offset = new(-Game1.viewport.X, -Game1.viewport.Y); 
			foreach (var particle in topParticles.Value)
			{
				particle.Offset = offset;
				particle.Draw(b);
			}
		}
		private static void Tick()
		{
			int millis = Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			foreach(var particle in bottomParticles.Value)
				particle.Tick(millis);
			foreach (var particle in topParticles.Value)
				particle.Tick(millis);
		}
		private static void ChangeLocation(GameLocation loc, bool soft)
		{
			List<IParticleManager> ps = new();
			foreach ((var tile, int x, int y) in loc.map.TilesInLayer("Back"))
			{
				if (tile.TileHasProperty("Particles", out var prop) && GenerateManager(prop, x, y, out var man))
				{
					ps.Add(man);
					man.Depth = y * tileDepth + .0032f;
					man.Tick(0);
				}
			}
			bottomParticles.Value = ps;
			ps = new();
			foreach ((var tile, int x, int y) in loc.map.TilesInLayer("AlwaysFront"))
			{
				if (tile.TileHasProperty("Particles", out var prop) && GenerateManager(prop, x, y, out var man))
				{
					ps.Add(man);
					man.Tick(0);
				}
			}
			topParticles.Value = ps;
		}
		private static bool GenerateManager(string prop, int x, int y, out IParticleManager manager)
		{
			manager = null;
			var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length < 1)
				return false;
			if (!Assets.Particles.TryGetValue(split[0], out var def))
				return false;
			if (!split.ToRect(out var rect, 1))
				rect = defaultRect;
			if (split.Length <= 6 || !int.TryParse(split[6], out int count))
				count = 100;
			if (split.Length <= 7 || !int.TryParse(split[7], out int rate))
				rate = 100;
			if (split.Length <= 8 || !int.TryParse(split[8], out int rateVar))
				rateVar = 0;
			if (split.Length <= 9 || !int.TryParse(split[9], out int burst))
				burst = 1;
			if (split.Length <= 10 || !int.TryParse(split[10], out int burstMax))
				burstMax = 1;
			manager = def.Create(new Emitter()
			{
				Region = new(x * 64 + rect.X * 4, y * 64 + rect.Y * 4, rect.Width * 4, rect.Height * 4),
				Rate = rate,
				RateVariance = rateVar,
				BurstMin = burst,
				BurstMax = burstMax,
				Radial = split.Length > 11
			}, count);
			return manager is not null;
		}
	}
}
