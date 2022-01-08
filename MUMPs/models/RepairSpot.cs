using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new(X*64 - 8, Y*64 - 64 + yOffset)), bubble, Color.White);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new(X*64 + 34, Y*64 - 24 + yOffset)), question, Color.White);
        }
    }
}
