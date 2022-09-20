using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MUMPs.UI
{
	class ImageDisplay : SimpleDisplay
	{
		public Texture2D Image;
		public Rectangle area;
		public ImageDisplay(Texture2D image) : base(image.Width * 3, image.Height * 3)
		{
			Image = image;
		}
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			b.Draw(Image, area, Color.White);
		}
		public override void resized()
		{
			area = new(xPositionOnScreen, yPositionOnScreen, width, height);
		}
	}
}
