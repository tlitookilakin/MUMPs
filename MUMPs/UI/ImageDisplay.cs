using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MUMPs.UI
{
    class ImageDisplay : SimpleDisplay
    {
        public Texture2D Image;
        public ImageDisplay(Texture2D image) : base(image.Width, image.Height)
        {
            Image = image;
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Image, new Vector2(xPositionOnScreen, yPositionOnScreen), Color.White);
        }
    }
}
