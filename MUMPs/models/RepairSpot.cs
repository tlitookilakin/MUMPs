using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs.models
{
    class RepairSpot
    {
        public static Rectangle bubble = new Rectangle(141, 465, 20, 24);
        public static Rectangle question = new Rectangle(175, 425, 12, 12);

        public int X = 0;
        public int Y = 0;

        public RepairSpot(int x, int y)
        {
            X = x;
            Y = y;
        }
        public void Draw(SpriteBatch b)
        {
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new(X*64 - 8, Y*64 - 128 + yOffset)), bubble, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new(X*64 + 8, Y*64 - 112 + yOffset)), question, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        }
    }
}
