using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace MUMPs.Props
{
	[ModInit]
	class DrawBorder
	{
		private static readonly PerScreen<bool> drawBorders = new(() => false);

		internal static void Init()
		{
			ModEntry.OnChangeLocation += ChangeLocation;
			ModEntry.OnCleanup += Cleanup;
			ModEntry.OnDraw += Draw;
		}
		private static void ChangeLocation(GameLocation loc, bool soft) 
			=> drawBorders.Value = !string.IsNullOrEmpty(loc.getMapProperty("DrawBorders"));
		private static void Draw(SpriteBatch b)
		{
			if (!drawBorders.Value || Game1.currentLocation == null)
				return;

			Rectangle view = Game1.viewport.ToRect();
			Rectangle map = new(0, 0, Game1.currentLocation.map.DisplayWidth, Game1.currentLocation.map.DisplayHeight);

			if (view.Y < 0)
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, view.Width, -view.Y), Color.Black);
			if (view.Bottom > map.Height)
				b.Draw(Game1.staminaRect, new Rectangle(0, map.Height - view.Y, view.Width, view.Bottom - map.Height), Color.Black);
			if (view.X < 0)
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, -view.X, view.Height), Color.Black);
			if (view.Right > map.Width)
				b.Draw(Game1.staminaRect, new Rectangle(map.Width - view.X, 0, view.Right - map.Width, view.Height), Color.Black);
		}
		private static void Cleanup() => drawBorders.ResetAllScreens();
	}
}
