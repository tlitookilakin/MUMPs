using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Props
{
    class Birds
    {
        //private static readonly Dictionary<string, PerchingBirds> flocks = new Dictionary<string, PerchingBirds>();
        private static PerchingBirds birds = null;
        private static Vector2 origin = new(8f, 14f);

        public static void EnterLocation(GameLocation location)
        {
            birds = null;
            string[] data = location.getMapProperty("Birds").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(data.Length < 1) { return; }
            List<Point> perches = new();
            Point empty = new(0, 0);
            for(int i = 1; i + 1 < data.Length; i += 2)
            {
                Point perch = Utils.StringsToPoint(data[i], data[i + 1]);
                if (perch != empty)
                {
                    perches.Add(perch);
                }
            }
            birds = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, origin, perches.ToArray(), perches.ToArray())
            {
                roosting = (Game1.timeOfDay >= 1800)
            };
            int count = Math.Clamp(int.Parse(data[0]), 0, perches.Count - 1);
            for(int i = 0; i < count; i++)
            {
                birds.AddBird((Game1.currentSeason == "fall") ? 10 : Game1.random.Next(0, 4));
            }
        }
        public static void Cleanup()
        {
            birds = null;
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
    }
}
