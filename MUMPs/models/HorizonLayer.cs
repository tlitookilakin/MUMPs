using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.models
{
    class HorizonLayer
    {
        public enum Mode {None, Tile, Stretch}
        public string texture { set; get; } = "";
        public Mode HMode { set; get; } = Mode.None;
        public Mode VMode { set; get; } = Mode.None;
        public float Depth { set; get; } = 0f;
        public float Opacity { set; get; } = 1f;
        public float Scale { set; get; } = 1f;
        public float OffsetX { set; get; } = 0f;
        public float OffsetY { set; get; } = 0f;
        public float MotionX { set; get; } = 0f;
        public float MotionY { set; get; } = 0f;

        private Texture2D cachedTexture = null;
        public Texture2D Texture
        {
            get
            {
                if(cachedTexture == null)
                {
                    try
                    {
                        ModEntry.helper.Content.Load<Texture2D>(texture);
                    } catch(Exception e)
                    {
                        ModEntry.monitor.Log("Could not load asset '"+texture+"': "+e.Message, LogLevel.Warn);
                        cachedTexture = Game1.fadeToBlackRect;
                    }
                }
                return cachedTexture;
            }
        }

        public void Draw(SpriteBatch b)
        {
            Point offset = new((int)MathF.Round(Game1.viewport.X * (1f - Depth) + OffsetX), (int)MathF.Round(Game1.viewport.Y * (1f - Depth) + OffsetY));
            switch (HMode)
            {
                case Mode.None:
                    DrawVmode(b, offset.X, offset.Y, (int)(Texture.Width * Scale));
                    break;
                case Mode.Stretch:
                    DrawVmode(b, offset.X, offset.Y, (int)Game1.currentLocation.map.DisplayWidth);
                    break;
                case Mode.Tile:
                    int tile = (int)(Texture.Width * Scale);
                    int wd = Game1.viewport.Width + Game1.viewport.X;
                    for(int xx = offset.X % tile; xx < offset.X + wd; xx += tile)
                    {
                        DrawVmode(b, xx, offset.Y, tile);
                    }
                    break;
            }
        }
        private void DrawVmode(SpriteBatch b, int x, int y, int w)
        {
            switch (VMode)
            {
                case Mode.None:
                    b.Draw(Texture, new Rectangle(x, y, w, (int)(Texture.Height * Scale)), Color.White * Opacity);
                    break;
                case Mode.Stretch:
                    b.Draw(Texture, new Rectangle(x, y, w, Game1.currentLocation.map.DisplayHeight), Color.White * Opacity);
                    break;
                case Mode.Tile:
                    int tile = (int)(Texture.Height * Scale);
                    int hg = Game1.viewport.Height + Game1.viewport.Y;
                    for (int yy = y % tile; yy < y + hg; yy += tile)
                    {
                        b.Draw(Texture, new Rectangle(x, yy, w, tile), Color.White * Opacity);
                    }
                    break;
            }
        }
    }
}
