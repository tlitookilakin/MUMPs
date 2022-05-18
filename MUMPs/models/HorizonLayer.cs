using AeroCore.Generics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        public string Texture { set; get; } = "";
        public Mode HMode { set; get; } = Mode.None;
        public Mode VMode { set; get; } = Mode.None;
        public float Depth { set; get; } = 0f;
        public float Opacity { set; get; } = 1f;
        public float Scale { set; get; } = 1f;
        public float OffsetX { set; get; } = 0f;
        public float OffsetY { set; get; } = 0f;
        public int MotionX { set; get; } = 0;
        public int MotionY { set; get; } = 0;
        public AnimationModel Animation { set; get; }
        public Rectangle Region { set; get; } = Rectangle.Empty;

        public readonly LazyAsset<Texture2D> ATexture;

        private int accum = 0;
        public HorizonLayer()
        {
            ATexture = new(ModEntry.helper, ModEntry.monitor, () => Texture);
        }
        public void Reload()
        {
            accum = 0;
        }
        private Rectangle getRegion(int millis)
        {
            Rectangle region = (Region == Rectangle.Empty) ? ATexture.Value.Bounds : Region;
            return Animation != null ? Animation.GetSource(region, millis) : region;
        }
        private int getScaledSize(int view, int map)
        {
            return (int)(view + (map - view) * (1f - Depth));
        }
        public void Draw(SpriteBatch b, Point center, int millis)
        {
            Point offset = new((int)(center.X * (1f - Depth) + OffsetX * 64f + .5f), (int)(center.Y * (1f - Depth) + OffsetY * 64f + .5f));
            Rectangle region = getRegion(millis);
            int fillx = getScaledSize(Game1.viewport.Width, Game1.currentLocation.map.DisplayWidth);
            int tile = (int)(region.Width * Scale);
            accum += millis;
            switch (HMode)
            {
                case Mode.None:
                    if(MotionX != 0)
                        DrawVmode(b, region, offset.X + (accum * MotionX / 1000 % (fillx + tile)) - tile, offset.Y, tile);
                    else
                        DrawVmode(b, region, offset.X, offset.Y, tile);
                    break;
                case Mode.Stretch:
                    DrawVmode(b, region, offset.X, offset.Y, fillx);
                    break;
                case Mode.Tile:
                    int wd = Game1.viewport.Width + offset.X;
                    for(int xx = (offset.X + accum * MotionX / 1000) % tile - tile; xx < wd; xx += tile)
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
                    b.Draw(ATexture.Value, new Rectangle(x, y, w, (int)(source.Height * Scale)), source, Color.White * Opacity);
                    break;
                case Mode.Stretch:
                    b.Draw(ATexture.Value, new Rectangle(x, y, w, getScaledSize(Game1.viewport.Height, Game1.currentLocation.map.DisplayHeight)), source, Color.White * Opacity);
                    break;
                case Mode.Tile:
                    int tile = (int)(source.Height * Scale);
                    int hg = Game1.viewport.Height + Game1.viewport.Y;
                    for (int yy = y % tile; yy < y + hg; yy += tile)
                    {
                        b.Draw(ATexture.Value, new Rectangle(x, yy, w, tile), source, Color.White * Opacity);
                    }
                    break;
            }
        }
    }
}
