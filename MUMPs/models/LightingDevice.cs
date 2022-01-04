using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;

namespace MUMPs.models
{
    class LightingDevice : XnaDisplayDevice
    {
        public Color Tint;
        public float Multiplier
        {
            set
            {
                mult = value;
                ModulationColour = Tint * value;
            }
            get
            {
                return mult;
            }
        }
        private float mult = 1f;

        public LightingDevice() : base(Game1.content, GameRunner.instance.GraphicsDevice)
        {
        }
    }
}
