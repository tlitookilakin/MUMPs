using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace MUMPs.Props
{
    class LightingLayer
    {
        private static Dictionary<TileSheet, Texture2D> sheets;
        public static void Reload()
        {
            if (ModEntry.helper.ModRegistry.IsLoaded("Platonymous.Toolkit"))
                try {
                    sheets = (Dictionary<TileSheet, Texture2D>)AccessTools.TypeByName("PyDisplayDevice").
                        GetField("m_tileSheetTextures2", BindingFlags.NonPublic | BindingFlags.Instance).
                        GetValue(Game1.mapDisplayDevice);
                    return;
                } catch (ArgumentException)
                {
                    ModEntry.monitor.Log("Failed to retrieve hijacked pytk display device, attempting default", LogLevel.Debug);
                }
            sheets = (Dictionary<TileSheet, Texture2D>)AccessTools.TypeByName("SXnaDisplayDevice").
                GetField("m_tileSheetTextures", BindingFlags.NonPublic | BindingFlags.Instance).
                GetValue(Game1.mapDisplayDevice);
        }
        public static void Draw(float intensity)
        {
            Layer layer = Game1.currentLocation?.map?.GetLayer("Lighting");
            if (layer == null)
                return;

            var batch = Game1.spriteBatch;
            Vector2 origin = Game1.GlobalToLocal(Vector2.Zero) / (Game1.options.lightingQuality / 2);
            float scale = 8f / Game1.options.lightingQuality;
            var size = Game1.lightmap.Bounds.Size;
            Vector2 pixoff = new(origin.X % 1f, origin.Y % 1f);
            int tilesize = (int)(scale * 16f);
            Point offset = new((int)(origin.X % tilesize), (int)(origin.Y % tilesize));
            Color color = intensity * Color.White;
            int tx = -(int)Math.Ceiling(origin.X / tilesize);
            int ty = -(int)Math.Ceiling(origin.Y / tilesize);

            for (int x = offset.X; x < size.X && tx < layer.LayerWidth; x += tilesize)
            {
                for(int y = offset.Y; y < size.Y && ty < layer.LayerHeight; y += tilesize)
                {
                    DrawTile(batch, layer.Tiles[new(tx, ty)], x, y, scale, color, pixoff);
                    ty++;
                }
                tx++;
                ty = -(int)(origin.Y / tilesize);
            }
        }
        public static void DrawTile(SpriteBatch b, Tile tile, int x, int y, float scale, Color color, Vector2 offset)
        {
            if(tile != null)
            {
                var src = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
                Texture2D tex = sheets[tile.TileSheet];
                if (!tex.IsDisposed)
                {
                    b.Draw(tex, new Vector2(x, y) + offset, new Rectangle(src.X, src.Y, src.Width, src.Height), color, 0f, Vector2.Zero, scale, SpriteEffects.None, .9f);
                }
            }
        }
    }
}
