using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace MUMPs.Props
{
	[ModInit]
	class Birds
	{
		private static readonly PerScreen<PerchingBirds> birds = new();
		private static readonly Vector2 origin = new(8f, 14f);

		internal static void Init()
		{
			ModEntry.OnChangeLocation += EnterLocation;
			ModEntry.OnCleanup += Cleanup;
			ModEntry.OnDraw += DrawAbove;
			ModEntry.OnTick += Update;
		}
		private static void EnterLocation(GameLocation location, bool soft)
		{
			birds.Value = null;
			var prop = location.getMapProperty("Birds").Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (prop.Length < 1)
				return;

			if (!int.TryParse(prop[0], out int count))
			{
				ModEntry.monitor.Log("Invalid value for 'Birds' property; could not convert to number.", LogLevel.Warn);
				return;
			}

			List<int> types = new();
			if (prop.Length < 2)
				if (Game1.currentSeason == "fall")
					types.Add(10);
				else
					types.AddRange(new[] {0, 1, 2, 3});
			else
				for (int i = 1; i < prop.Length; i++)
					if (int.TryParse(prop[i], out int ind))
						types.Add(ind);

			var (perch, roost) = GetSpotsFor(location);
			count = Math.Clamp(count, 0, Math.Min(perch.Length, roost.Length));

			birds.Value = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, origin, perch, roost)
			{
				roosting = Game1.timeOfDay >= 1800
			};

			for(int i = 0; i < count; i++)
				birds.Value.AddBird(Game1.random.Next(types));
		}
		private static void Cleanup() => birds.ResetAllScreens();
		private static void DrawAbove(SpriteBatch batch) => birds.Value?.Draw(batch);
		private static void Update() => birds.Value?.Update(Game1.currentGameTime);
		private static (Point[] perch, Point[] roost) GetSpotsFor(GameLocation location)
			=> location is null ? (Array.Empty<Point>(), Array.Empty<Point>()) : ScanForSpots(location);
		private static (Point[] perch, Point[] roost) ScanForSpots(GameLocation loc)
		{
			Map map = loc.Map;
			ModEntry.monitor.Log("Scanning for perches....");
			if (map == null)
				return (Array.Empty<Point>(), Array.Empty<Point>());

			Layer paths = map.GetLayer("Paths");
			if (paths == null)
				return (Array.Empty<Point>(), Array.Empty<Point>());

			List<Point> perches = new();
			List<Point> roosts = new();

			int w = paths.LayerWidth;
			int h = paths.LayerHeight;
			for (int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					Tile tile = paths.Tiles[new(x, y)];
					if (tile != null)
					{
						if (tile.TileHasProperty("Perch", out _))
							perches.Add(new(x, y));
						if (tile.TileHasProperty("Roost", out _))
							roosts.Add(new(x, y));
					}
				}
			}
			string p = null;
			foreach (var f in loc.furniture)
			{
				var fw = f.getTilesWide();
				var fh = f.getTilesHigh();
				for(int x = (int)f.TileLocation.X; x < fw; x++)
				{
					for(int y = (int)f.TileLocation.Y; y < fh; y++)
					{
						if (f.DoesTileHaveProperty(x, y, "Perch", "Paths", ref p))
							perches.Add(new(x, y));
						if (f.DoesTileHaveProperty(x, y, "Roost", "Paths", ref p))
							perches.Add(new(x, y));
					}
				}
			}
			ModEntry.monitor.Log($"Found {perches.Count} perches and {roosts.Count} roosts.");
			return (perches.ToArray(), roosts.ToArray());
		}
	}
}
