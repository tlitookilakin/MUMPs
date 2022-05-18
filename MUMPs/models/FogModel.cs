using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace MUMPs.models
{
    class FogModel
    {
        public static readonly Vector2 offset = new(32f, -32f);
        public float radius { get; set; } = 0f;
        public Color color { get; set; } = Color.Gray;

        private Texture2D fogTex;
        public FogModel(){}
        public FogModel(float Radius, Color Color)
        {
            radius = Radius;
            color = Color;
            fogTex = ModEntry.helper.Content.Load<Texture2D>("Mods/Mumps/Fog", ContentSource.GameContent);
        }
        public void Draw(SpriteBatch b)
        {
            var pos = Game1.GlobalToLocal(Game1.player.position) + offset;
            var port = Game1.viewport;
            Rectangle rect = new((int)(pos.X - radius + .5f), (int)(pos.Y - radius + .5f), (int)(2f * radius + .5f), (int)(2f * radius + .5f));
            if (pos.Y - radius > 0) // top
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, port.Width, rect.Y), color);
            if (pos.Y + radius < port.Height) // bottom
                b.Draw(Game1.staminaRect, new Rectangle(0, rect.Bottom, port.Width, port.Height - rect.Bottom), color);
            if (pos.X - radius > 0) // left
                b.Draw(Game1.staminaRect, new Rectangle(0, rect.Y, rect.X, rect.Height), color);
            if (pos.X + radius < port.Width) // right
                b.Draw(Game1.staminaRect, new Rectangle(rect.Right, rect.Y, port.Width - rect.Right, rect.Height), color);
            b.Draw(fogTex, rect, color);
        }
    }
}
