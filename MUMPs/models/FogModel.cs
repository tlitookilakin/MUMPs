using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.models
{
    class FogModel
    {
        public float radius { get; set; } = 0f;
        public float fade { get; set; } = 100f;
        public Color color { get; set; } = Color.Gray;

        private VertexBuffer mesh = null;
        private BasicEffect effect;

        public FogModel(){}
        public FogModel(float Radius, float Fade, Color Color)
        {
            radius = Radius;
            fade = Fade;
            color = Color;
            RegenerateMesh();
        }
        public void RegenerateMesh()
        {
            int iters = Math.Max(10, (int)((radius + fade) * MathF.Tau / 10) + 1);
            float turn = MathF.Tau / (iters - 1);
            var verts = new VertexPositionColor[iters * 2];
            for(int i = 0; i < iters; i++)
            {
                verts[i] = new(new(MathF.Cos(i * turn) * radius, MathF.Sin(i * turn) * radius, 0), Color.Transparent);
                verts[i + 1] = new(new(MathF.Cos(i * turn) * (radius + fade), MathF.Sin(i * turn) * (radius + fade), 0), color);
            }
            mesh = new(Game1.graphics.GraphicsDevice, typeof(VertexPositionColor), verts.Length, BufferUsage.WriteOnly);
            effect = new(Game1.graphics.GraphicsDevice);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height, 0, 0, 1);
            mesh.SetData(verts);
        }
        public void Draw(Vector2 pos)
        {
            Game1.spriteBatch.End();
            /*
            pos = Game1.GlobalToLocal(pos);
            effect.World = Matrix.CreateTranslation(pos.X, pos.Y, 0);
            effect.CurrentTechnique.Passes[0].Apply();
            Game1.graphics.GraphicsDevice.SetVertexBuffer(mesh);
            Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, mesh.VertexCount); 
            */
            var port = Game1.viewport;
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, port.Width, port.Height), color);
            Game1.spriteBatch.End();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        }
    }
}
