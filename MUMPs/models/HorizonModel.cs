using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
    class HorizonModel : IDrawableWorldLayer
    {
        public List<HorizonLayer> Layers { set; get; } = new();
        public bool DrawSky { set; get; } = true;

		private Color c = Color.White;
        public void Draw(SpriteBatch b, bool isForeground, Vector2 offset = default)
        {
            if(!isForeground && DrawSky)
				drawSky(b);

			Point center = new(
				(int)(offset.X * 64f) + (Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width) / 2 - Game1.viewport.X, 
				(int)(offset.Y * 64f) + (Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height) / 2 - Game1.viewport.Y
			);
			int millis = Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            foreach(var layer in Layers)
                layer.Draw(b, center, millis);
        }
		public void drawSky(SpriteBatch b)
        {
			float alpha = 1f;
			float skyAlpha = 1f;
			int adjustedTime = (int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66f);
			int seasonOffset = 0;
			float ablend = Game1.gameTimeInterval / 7000f * 16.6f;
			switch (Game1.currentSeason)
			{
				case "summer":
					seasonOffset = 0;
					break;
				case "fall":
					seasonOffset = 1;
					break;
				case "winter":
					seasonOffset = 2;
					break;
			}
			if (Game1.timeOfDay >= 1800)
			{
				c = new Color(255f, 255f - Math.Max(100f, adjustedTime + ablend - 1800f), 255f - Math.Max(100f, (adjustedTime + ablend - 1800f) / 2f));
				alpha = Math.Max(0f, Math.Min(1f, (2000f - (adjustedTime + ablend)) / 200f));
				skyAlpha = Math.Max(0f, Math.Min(1f, (2200f - (adjustedTime + ablend)) / 400f));
				Game1.ambientLight = new Color((int)Utility.Lerp(0f, 30f, 1f - alpha), (int)Utility.Lerp(0f, 60f, 1f - alpha), (int)Utility.Lerp(0f, 15f, 1f - alpha));
			}
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Rectangle(639, 858, 1, 144), c * skyAlpha, 0f, Vector2.Zero, SpriteEffects.None, 5E-08f);
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), (Game1.currentSeason == "fall") ? new Rectangle(639, 1051, 1, 400) : new Rectangle(639 + (seasonOffset + 1), 1051, 1, 400), c * alpha, 0f, Vector2.Zero, SpriteEffects.None, 1E-07f);
			if (Game1.timeOfDay >= 1800)
			{
				b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.viewport.Height / 2 - 780), new Rectangle(0, 1453, 638, 195), Color.White * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			if (Game1.dayOfMonth == 28 && Game1.timeOfDay > 1900)
			{
				b.Draw(Game1.mouseCursors, new Vector2((adjustedTime + ablend) / 2600f * Game1.viewport.Width / 4f, Game1.viewport.Height / 2 + 176 - (adjustedTime - 1900 + ablend) / 700f * Game1.viewport.Height / 2f), new Rectangle(642, 834, 43, 44), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 5E-08f);
			}
		}
    }
}
