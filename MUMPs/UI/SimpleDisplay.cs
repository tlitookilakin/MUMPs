using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace MUMPs.UI
{
    class SimpleDisplay : IClickableMenu
    {
        private static readonly Color shadow = Color.Black * 0.25f;
        public SimpleDisplay(int width, int height) : base(0, 0, width, height)
        {
            align();
            resized();
        }
        public override void receiveGamePadButton(Buttons b){exitThisMenu();}
        public override void receiveKeyPress(Keys key){exitThisMenu();}
        public override void receiveRightClick(int x, int y, bool playSound = true){exitThisMenu(playSound);}
        public override void receiveLeftClick(int x, int y, bool playSound = true){exitThisMenu(playSound);}
        public void align()
        {
            Point port = Game1.graphics.GraphicsDevice.Viewport.Bounds.Size;
            xPositionOnScreen = port.X / 2 - width / 2;
            yPositionOnScreen = port.Y / 2 - height / 2;
        }
        public virtual void resized() {}
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            align();
            resized();
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, shadow);
        }
    }
}
