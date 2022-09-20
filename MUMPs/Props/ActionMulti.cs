using AeroCore;
using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace MUMPs.Props
{
	[ModInit]
	internal class ActionMulti
	{
		internal static void Init()
		{
			ModEntry.AeroAPI.RegisterAction("Multi", HandleAction);
			ModEntry.AeroAPI.RegisterTouchAction("Multi", HandleTouchAction);
		}
		private static void HandleAction(Farmer who, string what, Point tile, GameLocation where)
		{
			var tloc = new xTile.Dimensions.Location(tile.X, tile.Y);
			foreach (var action in what.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				where.performAction(action, who, tloc);
		}
		private static void HandleTouchAction(Farmer who, string what, Point tile, GameLocation where)
		{
			var tloc = who.getTileLocation();
			foreach (var action in what.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				where.performTouchAction(action, tloc);
		}
	}
}
