using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MUMPs.Props
{
	[HarmonyPatch(typeof(Farmer))]
	[HarmonyPriority(Priority.High)]
	internal class Ramp
	{
		private static int offset;
		private static float oldX;

		[HarmonyPatch(nameof(Farmer.nextPosition))]
		[HarmonyPostfix]
		internal static void ApplyCheck(ref Rectangle __result, Farmer __instance, int direction)
		{
			oldX = __instance.Position.X;
			offset = 0;
			if (__instance is null)
				return;

			var loc = Game1.currentLocation;
			if (loc is null || !float.TryParse(loc.doesTileHavePropertyNoNull(__instance.getTileX(), __instance.getTileY(), "Ramp", "Back"), out var off))
				return;

			off *= __instance.getMovementSpeed();
			offset =
				direction is 1 ? (int)-off :
				direction is 3 ? (int)off :
				0;
			__result.Y += offset;
			//__result.Y += (int)((((direction >> ((~direction) & 1)) << 1) - 2) * off);
		}

		[HarmonyPatch(nameof(Farmer.nextPositionHalf))]
		[HarmonyPostfix]
		internal static void ApplyCheckHalf(ref Rectangle __result, Farmer __instance, int direction)
		{
			oldX = __instance.Position.X;
			offset = 0;
			if (__instance is null)
				return;

			var loc = Game1.currentLocation;
			if (loc is null || !float.TryParse(loc.doesTileHavePropertyNoNull(__instance.getTileX(), __instance.getTileY(), "Ramp", "Back"), out var off))
				return;

			off *= __instance.getMovementSpeed() * .5f;
			offset =
				direction is 1 ? (int)-off :
				direction is 3 ? (int)off :
				0;
			__result.Y += offset;
		}

		[HarmonyPatch(nameof(Farmer.MovePosition))]
		[HarmonyPrefix]
		[HarmonyPriority(Priority.High)]
		internal static void Reset()
			=> offset = 0;

		[HarmonyPatch(nameof(Farmer.MovePosition))]
		[HarmonyPostfix]
		internal static void ApplyModifier(Farmer __instance)
		{
			if (offset is not 0 && oldX is not 0 && oldX != __instance.Position.X)
				__instance.position.Y += offset;
		}
	}
}
