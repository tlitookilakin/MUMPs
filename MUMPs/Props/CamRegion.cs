using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
	[HarmonyPatch]
	[ModInit]
	class CamRegion
	{
		private static readonly PerScreen<List<Rectangle>> regions = new(() => new());
		internal static void Init()
		{
			ModEntry.OnChangeLocation += ChangeLocation;
			ModEntry.OnCleanup += Cleanup;
		}
		private static void ChangeLocation(GameLocation loc)
		{
			regions.Value.Clear();
			string[] split = loc.getMapProperty("CamRegions")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (split == null)
				return;

			for(int i = 0; i + 3 < split.Length; i += 4)
			{
				if(!split.ToRect(out Rectangle rect, i))
				{
					ModEntry.monitor.Log($"Failed to parse CamRegion map property @ '{loc.mapPath.Value}': could not convert to number.", LogLevel.Warn);
					regions.Value.Clear();
					return;
				}
				regions.Value.Add(rect);
			}
		}
		private static void Cleanup() => regions.ResetAllScreens();

		[HarmonyPatch(typeof(Game1), "UpdateViewPort")]
		[HarmonyPrefix]
		internal static void UpdateCamera(bool overrideFreeze, ref Point centerPoint)
		{
			if (Game1.currentLocation.forceViewportPlayerFollow || (!overrideFreeze && Game1.viewportFreeze))
				return;

			Point tileCenter = new(centerPoint.X / 64, centerPoint.Y / 64);
			foreach(var region in regions.Value)
				if (region.Contains(tileCenter))
				{
					centerPoint.X = (Game1.viewport.Width >= region.Width) ? region.X + region.Width / 2 :
						Math.Clamp(centerPoint.X, region.X + Game1.viewport.Width / 2, region.X + region.Width - Game1.viewport.Width / 2);
					centerPoint.Y = (Game1.viewport.Height >= region.Height) ? region.Y + region.Height / 2 :
						Math.Clamp(centerPoint.Y, region.Y + Game1.viewport.Height / 2, region.Y + region.Height - Game1.viewport.Height / 2);
					break;
				}
		}
	}
}
