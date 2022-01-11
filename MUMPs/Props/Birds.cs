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
    class Birds
    {
        private static readonly PerScreen<PerchingBirds> birds = new();
        private static readonly Vector2 origin = new(8f, 14f);
        private static readonly PerScreen<Dictionary<GameLocation, (Point[] perch, Point[] roost)>> spots = new(() => new());

        public static void EnterLocation(GameLocation location)
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

            var spots = GetSpotsFor(location);
            count = Math.Clamp(count, 0, Math.Min(spots.perch.Length, spots.roost.Length));

            birds.Value = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, origin, spots.perch, spots.roost)
            {
                roosting = (Game1.timeOfDay >= 1800)
            };
            for(int i = 0; i < count; i++)
            {
                birds.Value.AddBird((Game1.currentSeason == "fall") ? 10 : Game1.random.Next(0, 4));
            }
        }
        public static void StartDay()
        {
            spots.Value.Clear();
        }
        public static void Cleanup()
        {
            birds.ResetAllScreens();
            spots.ResetAllScreens();
        }
        public static void DrawAbove(SpriteBatch batch)
        {
            if(birds.Value == null)
                return;
            birds.Value.Draw(batch);
        }
        public static void Update(GameTime time)
        {
            if(birds.Value == null)
                return;
            birds.Value.Update(time);
        }
        public static (Point[] perch, Point[] roost) GetSpotsFor(GameLocation location)
        {

            if (location == null)
                return (Array.Empty<Point>(), Array.Empty<Point>());

            if (!spots.Value.TryGetValue(location, out var spot)){
                spot = ScanForSpots(location.Map);
                spots.Value.Add(location, spot);
            }

            return spot;
        }
        public static (Point[] perch, Point[] roost) ScanForSpots(Map map)
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
            ModEntry.monitor.Log("Found " + perches.Count.ToString() + " perches and " + roosts.Count.ToString() + " roosts.");
            return (perches.ToArray(), roosts.ToArray());
        }
    }
}
