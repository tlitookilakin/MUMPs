using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs.Patches
{
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
	internal class DepthDraw
	{
		[HarmonyPostfix]
		internal static void Post(SpriteBatch b)
		{
			ModEntry.EmitDepthDraw(b);
		}
	}
}
