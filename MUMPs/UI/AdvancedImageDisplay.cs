using AeroCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MUMPs.UI
{
    internal class AdvancedImageDisplay : SimpleDisplay
    {
        private readonly AnimatedImage Image;
        private Rectangle area;
        public AdvancedImageDisplay(AnimatedImage image) : base(100, 100)
        {
            Image = image;
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Image.Animate(Game1.currentGameTime.ElapsedGameTime.Milliseconds);
            (var region, var texture) = Image.GetDrawable();
            bool resize = width != region.Width * 3 || height != region.Height * 3;
            width = region.Width * 3;
            height = region.Height * 3;
            if (resize)
            {
                align();
                resized();
            }
            b.Draw(texture, area, region, Color.White);
        }
        public override void resized()
        {
            area = new(xPositionOnScreen, yPositionOnScreen, width, height);
        }
    }
}
