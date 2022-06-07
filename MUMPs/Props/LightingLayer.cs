using AeroCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;
using AeroCore.Utils;
using AeroCore.API;
using AeroCore;

namespace MUMPs.Props
{
    [ModInit]
    class LightingLayer
    {
        private static Dictionary<TileSheet, Texture2D> sheets;

        internal static void Init()
        {
            ModEntry.AeroAPI.LightingEvent += Draw;
        }
        internal static void Reload()
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
        private static void Draw(ILightingEventArgs ev)
        {
            Layer layer = Game1.currentLocation?.map?.GetLayer("Lighting");
            if (layer == null)
                return;

            var batch = Game1.spriteBatch;
            Color color = ev.intensity * Color.White;
            float scale = ev.scale * 4f;
            int tilesize = (int)(scale * 16f);
            var port = Game1.viewport.ToRect();
            Point offset = new((int)(port.X % 64 * ev.scale), (int)(port.Y % 64 * ev.scale));
            port = new(port.X / 64, port.Y / 64, port.Width / 64 + 1, port.Height / 64 + 1);

            for(int x = 0; x < port.Width; x++)
                for(int y = 0; y < port.Height; y++)
                    DrawTile(batch, layer.Tiles[new(x + port.X, y + port.Y)], x * tilesize - offset.X, y * tilesize - offset.Y, scale, color);
        }
        private static void DrawTile(SpriteBatch b, Tile tile, int x, int y, float scale, Color color)
        {
            if(tile != null)
            {
                Texture2D tex = sheets[tile.TileSheet];
                if (!tex.IsDisposed)
                {
                    b.Draw(tex, new Vector2(x, y), tile.TileSheet.GetTileImageBounds(tile.TileIndex).ToRect(),
                        color, 0f, Vector2.Zero, scale, SpriteEffects.None, .9f);
                }
            }
        }
    }
}
