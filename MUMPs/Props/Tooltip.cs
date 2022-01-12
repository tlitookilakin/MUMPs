﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MUMPs.Props
{
    class Tooltip
    {
        private static readonly Point offset = new(48, 48);
        private static readonly Rectangle bgSrc = new(403, 373, 9, 9);
        public static void Draw(SpriteBatch b)
        {
            if (Game1.currentLocation == null || Game1.activeClickableMenu != null)
                return;

            Point pos = Utils.LocalToGlobal(Game1.getMouseX(), Game1.getMouseY());
            string tip = Game1.currentLocation.doesTileHavePropertyNoNull(pos.X / 64, pos.Y / 64, "Tooltip", "Buildings");

            if (tip == null)
                return;
            tip = tip.Trim();
            if (tip.Length == 0)
                return;

            DrawTip(b, tip);
        }
        public static void DrawTip(SpriteBatch b, string tip)
        {
            var ms = Game1.getMousePositionRaw();
            var size = Game1.smallFont.MeasureString(tip);
            Rectangle box = new(ms.X + offset.X, ms.Y + offset.Y, (int)size.X + 18, (int)size.Y + 18);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, bgSrc, box.X, box.Y, box.Width, box.Height, Color.White, 3f);
            b.DrawString(Game1.smallFont, tip, new(box.X + 9, box.Y + 9), Game1.textColor);
        }
    }
}
