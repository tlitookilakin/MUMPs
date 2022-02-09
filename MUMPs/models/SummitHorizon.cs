using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs.models
{
    class SummitHorizon : IDrawableWorldLayer
    {
        private readonly Background bg = new();
        public void Draw(SpriteBatch b, bool isForeground, Vector2 offset = default)
        {
            bg.draw(b);
        }
    }
}
