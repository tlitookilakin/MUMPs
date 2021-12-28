using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs.models
{
    class SummitHorizon : IDrawableWorldLayer
    {
        private readonly Background bg = new();
        public void Draw(SpriteBatch b, bool isForeground)
        {
            bg.draw(b);
        }
    }
}
