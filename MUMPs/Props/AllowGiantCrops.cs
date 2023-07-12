using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MUMPs.Props
{
	[ModInit]
	internal class AllowGiantCrops
	{
		const int buffer = 5;

		internal static void Init()
		{
			if (ModEntry.helper.ModRegistry.IsLoaded("leclair.giantcroptweaks"))
				return; // that mod already does this

			ModEntry.harmony.Patch(typeof(Crop).MethodNamed(nameof(Crop.newDay)), 
				transpiler: new(typeof(AllowGiantCrops).MethodNamed(nameof(Transpiler))));
		}
		internal static IEnumerable<CodeInstruction> wrap(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in Transpiler(instructions))
			{
				ModEntry.monitor.Log(instruction.ToString(), StardewModdingAPI.LogLevel.Debug);
				yield return instruction;
			}
		}
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> source)
		{
			IEnumerator<CodeInstruction> cursor = source.GetEnumerator();
			bool flagged = false;

			while (cursor.MoveNext())
			{
				var c = cursor.Current;

				if (flagged && (c.opcode == OpCodes.Isinst || c.Branches(out _)))
					yield return new(OpCodes.Call, typeof(AllowGiantCrops).MethodNamed(nameof(CheckProperty)));
				flagged = !flagged && c.opcode == OpCodes.Ldarg_S && (byte)c.operand == 5;
				if (c.opcode != OpCodes.Isinst || c.operand is not Type t || t != typeof(Farm))
					yield return c;
			}
		}
		private static GameLocation CheckProperty(GameLocation where)
		{
			if (where is Farm or IslandWest or null)
				return where;
			var prop = where.getMapProperty("AllowGiantCrops");
			return prop is not null && prop.Length > 0 ? where : null;
		}
	}
}
