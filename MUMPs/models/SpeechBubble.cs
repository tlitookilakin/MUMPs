using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace MUMPs.models
{
	internal struct SpeechBubble
	{
		public string Text;
		public Vector2 Position;
		public int Timer => timer;
		private int timer;

		public SpeechBubble(string text, Vector2 position)
		{
			Text = text;
			Position = position;
			timer = 3000;
		}

		public void Draw(SpriteBatch b, int millis)
		{
			timer -= millis;
			float alpha = MathF.Min(timer / 500f, 1f);
			var pos = Game1.GlobalToLocal(Position);
			SpriteText.drawStringWithScrollCenteredAt(b, Text, (int)pos.X, (int)pos.Y, "", alpha, -1, 1, 
				Position.X / 10000f + 0.001f + Position.Y / 10000f);
		}
	}
}
