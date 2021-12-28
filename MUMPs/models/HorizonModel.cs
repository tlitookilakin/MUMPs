using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
    class HorizonModel : IDrawableWorldLayer
    {
        public List<HorizonLayer> Layers { set; get; } = new();
        public float OffsetX { set; get; } = 0f;
        public float OffsetY { set; get; } = 0f;
        public bool DrawSky { set; get; } = true;

        public void Draw(SpriteBatch b, bool isForeground)
        {
            if(!isForeground && DrawSky)
            {
                //draw sky
            }
            foreach(var layer in Layers)
            {
                layer.Draw(b);
            }
        }
    }
}
