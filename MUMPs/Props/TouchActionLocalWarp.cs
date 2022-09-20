using AeroCore;
using Microsoft.Xna.Framework;
using StardewValley;
using AeroCore.Utils;
using System;

namespace MUMPs.Props
{
	[ModInit]
	internal class TouchActionLocalWarp
	{
		internal void Init()
		{
			ModEntry.AeroAPI.RegisterTouchAction("LocalWarp", HandleTouchAction);
			ModEntry.AeroAPI.RegisterAction("LocalWarp", HandleAction);
		}
		private void HandleTouchAction(Farmer who, string what, Point tile, GameLocation where)
		{
			var split = what.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (!split.ToVector2(out var to))
				return;

			if (split.Length > 2)
			{
				Game1.afterFade = () => who.setTileLocation(to);
				Game1.fadeScreenToBlack();
			} else
			{
				who.setTileLocation(to);
			}
		}
		private void HandleAction(Farmer who, string what, Point tile, GameLocation where)
		{
			where.playSoundAt("doorClose", tile.ToVector2());
			HandleTouchAction(who, what, tile, where);
		}
	}
}
