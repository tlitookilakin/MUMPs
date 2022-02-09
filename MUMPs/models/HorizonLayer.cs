using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.Misc;
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
        public AnimationModel Animation { set; get; }
        public Rectangle Region { set; get; } = Rectangle.Empty;

        private Texture2D cachedTexture = null;
        public readonly RLazy<Texture2D> Texture;

        public HorizonLayer()
        {
            Texture = new(TryLoadTexture);
        }
        private Texture2D TryLoadTexture()
        {
            try
            {
                return ModEntry.helper.Content.Load<Texture2D>(texture);
            }
            catch (ContentLoadException e)
            {
                ModEntry.monitor.Log("Could not load asset '" + texture + "': " + e.Message, LogLevel.Warn);
                return Game1.fadeToBlackRect;
            }
        }
        private Rectangle getRegion()
        {
            Rectangle region = (Region == Rectangle.Empty) ? Texture.Value.Bounds : Region;
            return Animation != null ? Animation.GetSource(region, Game1.currentGameTime.ElapsedGameTime.Milliseconds) : region;
        }
        private int getScaledSize(int view, int map)
        {
            return (int)(view + (map - view) * (1f - Depth));
        }
        public void Draw(SpriteBatch b, Point center)
        {
            Point offset = new((int)MathF.Round(Game1.viewport.X * (1f - Depth) + OffsetX + center.X), (int)MathF.Round(Game1.viewport.Y * (1f - Depth) + OffsetY + center.Y));
            Rectangle region = getRegion();
            switch (HMode)
            {
                case Mode.None:
                    DrawVmode(b, region, offset.X, offset.Y, (int)(region.Width * Scale));
                    break;
                case Mode.Stretch:
                    DrawVmode(b, region, offset.X, offset.Y, getScaledSize(Game1.viewport.Width, Game1.currentLocation.map.DisplayWidth));
                    break;
                case Mode.Tile:
                    int tile = (int)(region.Width * Scale);
                    int wd = Game1.viewport.Width + Game1.viewport.X;
                    for(int xx = offset.X % tile; xx < offset.X + wd; xx += tile)
                    {
                        DrawVmode(b, region, xx, offset.Y, tile);
                    }
                    break;
            }
        }
        private void DrawVmode(SpriteBatch b, Rectangle source, int x, int y, int w)
        {
            switch (VMode)
            {
                case Mode.None:
                    b.Draw(Texture.Value, new Rectangle(x, y, w, (int)(source.Height * Scale)), source, Color.White * Opacity);
                    break;
                case Mode.Stretch:
                    b.Draw(Texture.Value, new Rectangle(x, y, w, getScaledSize(Game1.viewport.Height, Game1.currentLocation.map.DisplayHeight)), source, Color.White * Opacity);
                    break;
                case Mode.Tile:
                    int tile = (int)(source.Height * Scale);
                    int hg = Game1.viewport.Height + Game1.viewport.Y;
                    for (int yy = y % tile; yy < y + hg; yy += tile)
                    {
                        b.Draw(Texture.Value, new Rectangle(x, yy, w, tile), source, Color.White * Opacity);
                    }
                    break;
            }
        }
    }
}
