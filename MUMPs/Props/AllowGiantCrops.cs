﻿using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
	[HarmonyPatch(typeof(Crop), nameof(Crop.newDay))]
	internal class AllowGiantCrops
	{
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

			// skip 3
			for(int i = 2; i > 0 && cursor.MoveNext(); i--)
				yield return cursor.Current;

			// add label to last one
			if (!cursor.MoveNext())
				yield break;
			cursor.Current.labels.Add(jump);
			yield return cursor.Current;

			// remove casting if atra hasn't already
			if (ModEntry.helper.ModRegistry.IsLoaded("atravita.GiantCropFertilizer"))
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
