using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;

namespace MUMPs.models
{
    class LightingDevice : IDisplayDevice, IDisposable
    {
        private SpriteBatch batch;
        private Dictionary<TileSheet, Texture2D> m_tileSheetTextures;
        private Microsoft.Xna.Framework.Rectangle m_scissorRectangle;
        private Vector2 m_tilePosition;
        private Microsoft.Xna.Framework.Rectangle m_sourceRectangle;
        public Color ModulationColour;

        public LightingDevice()
        {
            var graphicsDevice = GameRunner.instance.GraphicsDevice;
            m_tileSheetTextures = new Dictionary<TileSheet, Texture2D>();
            m_scissorRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            m_tilePosition = default(Vector2);
            m_sourceRectangle = default(Microsoft.Xna.Framework.Rectangle);
            ModulationColour = Color.White;
        }

        public void BeginScene(SpriteBatch b)
        {
            batch = b;
        }
        public void DisposeTileSheet(TileSheet tileSheet)
        {
            if (m_tileSheetTextures.ContainsKey(tileSheet))
            {
                m_tileSheetTextures.Remove(tileSheet);
            }
        }
        public void DrawTile(Tile tile, Location location, float layerDepth)
        {
            if (tile != null)
            {
                xTile.Dimensions.Rectangle tileImageBounds = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
                Texture2D texture = m_tileSheetTextures[tile.TileSheet];
                m_tilePosition.X = location.X;
                m_tilePosition.Y = location.Y;
                m_sourceRectangle.X = tileImageBounds.X;
                m_sourceRectangle.Y = tileImageBounds.Y;
                m_sourceRectangle.Width = tileImageBounds.Width;
                m_sourceRectangle.Height = tileImageBounds.Height;
                batch.Draw(texture, m_tilePosition, m_sourceRectangle, ModulationColour);
            }
        }
        public void EndScene()
        {
            batch = null;
        }
        public void LoadTileSheet(TileSheet tileSheet)
        {
            Texture2D value = Game1.content.Load<Texture2D>(tileSheet.ImageSource);
            m_tileSheetTextures[tileSheet] = value;
        }
        public void SetClippingRegion(xTile.Dimensions.Rectangle clippingRegion)
        {
            //what is this used for? doesn't show up in the IL code????
        }
        ~LightingDevice()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (batch != null)
                {
                    batch.Dispose();
                    batch = null;
                }
                m_tileSheetTextures.Clear();
            }
        }
    }
}
