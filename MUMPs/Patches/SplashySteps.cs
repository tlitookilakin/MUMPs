using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MUMPs.Patches
{
	[HarmonyPatch(typeof(FarmerSprite),"checkForFootstep")]
	class SplashySteps
	{
		private static readonly Color splashColor = new(141, 181, 216, 91);

		private static ILHelper patcher = new ILHelper(ModEntry.monitor, "Splashy Steps")
			.SkipTo(new CodeInstruction[]{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, typeof(FarmerSprite).FieldNamed("currentStep")),
				new(OpCodes.Stloc_2)
			})
			.Skip(3)
			.Add(new CodeInstruction[]{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, typeof(FarmerSprite).FieldNamed("owner")),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, typeof(SplashySteps).MethodNamed(nameof(shouldUseSplash))),
				new(OpCodes.Stloc_2)
			})
			.SkipTo(new CodeInstruction[]
			{
				new(OpCodes.Ldloc_2),
				new(OpCodes.Call, typeof(Game1).MethodNamed(nameof(Game1.playSound)))
			})
			.Skip(2)
			.Add(new CodeInstruction[]
			{
				new(OpCodes.Ldloc_2),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, typeof(FarmerSprite).FieldNamed("owner")),
				new(OpCodes.Call, typeof(SplashySteps).MethodNamed(nameof(addRipple)))
			})
			.Finish();
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => patcher.Run(instructions);
		private static string shouldUseSplash(Farmer who, FarmerSprite sprite)
		{
			var pos = who.getTileLocationPoint();
			var loc = who.currentLocation;

			return (
				loc is not BoatTunnel && 
				loc.getMapProperty("NoSplashSteps").Length is 0 &&
				loc.doesTileHaveProperty(pos.X, pos.Y, "Water", "Back") is not null && 
				loc.getTileIndexAt(pos, "Buildings") is -1
				) 
				? "quickSlosh" 
				: sprite.currentStep;
		}
		private static void addRipple(string what, Farmer who)
		{
			if (what != "quickSlosh" || who?.currentLocation is null)
				return;

			who.currentLocation.TemporarySprites.Add(
				new("TileSheets\\animations", new(0, 0, 64, 64), Game1.random.Next(50, 100), 9, 1, who.Position, flicker: false,
				flipped: false, 0f, 0f, splashColor, 1f, 0f, 0f, 0f));
			who.currentLocation.TemporarySprites.Add(
				new("TileSheets\\animations", new(128, 1152, 64, 64), Game1.random.Next(75, 125), 5, 1, new(who.Position.X, who.Position.Y - 32f),
				flicker: false, flipped: false, 0f, 0f, splashColor, 1f, 0f, 0f, 0f));
		}
	}
}
