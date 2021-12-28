using Microsoft.Xna.Framework.Graphics;

namespace MUMPs.models
{
    interface IDrawableWorldLayer
    {
        public void Draw(SpriteBatch b, bool isForeground);
    }
}
