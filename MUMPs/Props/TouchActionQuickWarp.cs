using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace MUMPs.Props
{
	[ModInit]
	internal class TouchActionQuickWarp
	{
		internal static void Init()
		{
			ModEntry.AeroAPI.RegisterTouchAction("QuickWarp", HandleWarp);
		}
		private static void HandleWarp(Farmer who, string action, Point tile, GameLocation where)
		{
			var split = action.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length < 3 || !int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y))
				return;
			Maps.QuickWarp(split[0], x, y, false);
		}
	}
}
