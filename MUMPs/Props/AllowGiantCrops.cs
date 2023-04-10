using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
	[ModInit]
	internal class AllowGiantCrops
	{
		internal static void Init()
		{
			if (ModEntry.helper.ModRegistry.IsLoaded("leclair.giantcroptweaks"))
				return; // that mod already does this

			ModEntry.harmony.Patch(typeof(Crop).MethodNamed(nameof(Crop.newDay)), 
				transpiler: new(typeof(AllowGiantCrops).MethodNamed(nameof(Transpiler))));
		}
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> source, ILGenerator gen)
		{
			IEnumerator<CodeInstruction> cursor = source.GetEnumerator();
			var flag = typeof(Crop).FieldNamed(nameof(Crop.phaseToShow));
			var jump = gen.DefineLabel();

			// find the first request for the location after PhaseToShow
			bool flagged = false;
			while (cursor.MoveNext()) {
				if (!flagged && cursor.Current.LoadsField(flag))
					flagged = true;
				else if (flagged && cursor.Current.opcode == OpCodes.Ldarg_S)
					break;
				yield return cursor.Current;
			}

			// if the map property is present and non-empty, skip other checks
			yield return new(cursor.Current.opcode, cursor.Current.operand);
			yield return new(OpCodes.Ldstr, "AllowGiantCrops");
			yield return new(OpCodes.Callvirt, typeof(GameLocation).MethodNamed(nameof(GameLocation.getMapProperty)));
			yield return new(OpCodes.Callvirt, typeof(string).PropertyGetter(nameof(string.Length)));
			yield return new(OpCodes.Ldc_I4_0);
			yield return new(OpCodes.Bgt_S, jump);
			yield return cursor.Current;

			// skip env check chunk
			if (cursor.TryGetNext(out var op))
				if (op.opcode != OpCodes.Isinst) // inst check already removed
					yield return op;
				else if (cursor.TryGetNext(out op)) // remove inst checks
					yield return op;
				else
					yield break;
			else
				yield break;

			// add label to last one
			if (!cursor.MoveNext())
				yield break;
			cursor.Current.labels.Add(jump);
			yield return cursor.Current;

			// remove further casting if it hasn't been already
			if (ModEntry.helper.ModRegistry.IsLoaded("atravita.GiantCropFertilizer") || 
				ModEntry.helper.ModRegistry.IsLoaded("spacechase0.MoreGiantCrops"))
			{
				while(cursor.MoveNext())
					yield return cursor.Current;
			} else
			{
				while (cursor.MoveNext())
					if (cursor.Current.opcode != OpCodes.Isinst || !typeof(Farm).Equals(cursor.Current.operand as Type))
						yield return cursor.Current;
			}
		}
	}
}
