using AeroCore;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace MUMPs.Props
{
	[ModInit]
	internal class TouchActionPlaySound
	{
		internal static void Init()
		{
			ModEntry.AeroAPI.RegisterTouchAction("playSound", HandleTouchAction);
		}
		private static void HandleTouchAction(Farmer who, string what, Point tile, GameLocation where)
		{
			try
			{
				where.playSoundAt(what.Trim(), tile.ToVector2());
			} catch (Exception e)
			{
				ModEntry.monitor.Log($"Could not play sound cue '{what.Trim()}'.", LogLevel.Warn);
				ModEntry.monitor.Log(e.Message);
			}
		}
	}
}
