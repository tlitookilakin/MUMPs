using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
        private static PerchingBirds birds = null;
        private static Vector2 origin = new(8f, 14f);
        private static readonly Dictionary<GameLocation, (Point[] perch, Point[] roost)> spots = new();

        public static void EnterLocation(GameLocation location)
        {

            birds = null;

            if (!int.TryParse(location.getMapProperty("Birds"), out int count))
            {
                ModEntry.monitor.Log("Invalid value for 'Birds' property; could not convert to number.");
                return;
            }

            var spots = GetSpotsFor(location);
            count = Math.Clamp(count, 0, Math.Min(spots.perch.Length, spots.roost.Length));

            birds = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, origin, spots.perch, spots.roost)
            {
                roosting = (Game1.timeOfDay >= 1800)
            };
            for(int i = 0; i < count; i++)
            {
                birds.AddBird((Game1.currentSeason == "fall") ? 10 : Game1.random.Next(0, 4));
            }
        }
        public static void StartDay()
        {
            spots.Clear();
        }
        public static void Cleanup()
        {
            birds = null;
            spots.Clear();
        }
        public static void DrawAbove(SpriteBatch batch)
        {
            if(birds == null) { return; }
            birds.Draw(batch);
        }
        public static void Update(GameTime time)
        {
            if(birds == null) { return; }
            birds.Update(time);
        }
        public static (Point[] perch, Point[] roost) GetSpotsFor(GameLocation location)
        {

            if (location == null)
                return (Array.Empty<Point>(), Array.Empty<Point>());

            if (!spots.TryGetValue(location, out var spot)){
                spot = ScanForSpots(location.Map);
                spots.Add(location, spot);
            }

            return spot;
        }
        public static (Point[] perch, Point[] roost) ScanForSpots(Map map)
        {
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
                        if (tile.Properties.ContainsKey("perch"))
                            perches.Add(new(x, y));
                        if (tile.Properties.ContainsKey("roost"))
                            roosts.Add(new(x, y));
                    }
                }
            }

            return (perches.ToArray(), roosts.ToArray());
        }
    }
}
