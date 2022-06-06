using AeroCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
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
        private static void EnterLocation(GameLocation location)
        {
            birds.Value = null;
            string prop = location.getMapProperty("Birds");
            if (prop.Length == 0)
                return;

            if (!int.TryParse(prop, out int count))
            {
                ModEntry.monitor.Log("Invalid value for 'Birds' property; could not convert to number.", LogLevel.Warn);
                return;
            }

            var (perch, roost) = GetSpotsFor(location);
            count = Math.Clamp(count, 0, Math.Min(perch.Length, roost.Length));

            birds.Value = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, origin, perch, roost)
            {
                roosting = Game1.timeOfDay >= 1800
            };

            for(int i = 0; i < count; i++)
                birds.Value.AddBird((Game1.currentSeason == "fall") ? 10 : Game1.random.Next(0, 4));
        }
        private static void Cleanup() => birds.ResetAllScreens();
        private static void DrawAbove(SpriteBatch batch) => birds.Value?.Draw(batch);
        private static void Update() => birds.Value?.Update(Game1.currentGameTime);
        private static (Point[] perch, Point[] roost) GetSpotsFor(GameLocation location)
            => location is null ? (Array.Empty<Point>(), Array.Empty<Point>()) : ScanForSpots(location.map);
        private static (Point[] perch, Point[] roost) ScanForSpots(Map map)
        {
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
                        if (tile.Properties.ContainsKey("Perch"))
                            perches.Add(new(x, y));
                        if (tile.Properties.ContainsKey("Roost"))
                            roosts.Add(new(x, y));
                    }
                }
            }
            ModEntry.monitor.Log($"Found {perches.Count} perches and {roosts.Count} roosts.");
            return (perches.ToArray(), roosts.ToArray());
        }
    }
}
