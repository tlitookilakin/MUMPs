using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
	[HarmonyPatch(typeof(Character), "applyVelocity")]
	[HarmonyPriority(Priority.High)]
	internal class Ramp
	{
		private static ILHelper patch = new ILHelper(ModEntry.monitor, "Ramp")
			.SkipTo(new CodeInstruction[]
			{
				new(OpCodes.Ldfld, typeof(Character).FieldNamed(nameof(Character.yVelocity))),
				new(OpCodes.Conv_I4)
			})
			.Skip(2)
			.Add(new CodeInstruction[]
			{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_1),
				new(OpCodes.Call, typeof(Ramp).MethodNamed(nameof(SlopeOffset)))
			})
			.Finish();

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> source)
			=> patch.Run(source);

		private static int SlopeOffset(int v, Character inst, GameLocation loc)
			=> inst is not null && float.TryParse(loc.doesTileHaveProperty(inst.getTileX(), inst.getTileY(), "Ramp", "Back"), out var off) ?
			v + (int)(off * inst.xVelocity) : v;
	}
}
