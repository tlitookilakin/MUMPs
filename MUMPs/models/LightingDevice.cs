using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Display;

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
