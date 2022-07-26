using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace MUMPs.Patches
{
    [HarmonyPatch(typeof(SObject), nameof(SObject.placementAction))]
    class FarmhouseFurnitureFix
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            => patcher.Run(instructions, generator);

        private static readonly ILHelper patcher = new ILHelper(ModEntry.monitor, "Furniture placement fix")
            .SkipTo(new CodeInstruction(OpCodes.Call, typeof(Furniture).MethodNamed(nameof(Furniture.GetFurnitureInstance))))
            .Skip(1)
            .RemoveTo(new CodeInstruction[]
            {
                new(OpCodes.Isinst, typeof(Furniture)),
                new(OpCodes.Callvirt, typeof(Furniture).MethodNamed(nameof(Furniture.updateRotation))),
                new(OpCodes.Ldc_I4_1)
            })
            .Remove(3)
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldloca_S, 31),
                new(OpCodes.Call, typeof(FarmhouseFurnitureFix).MethodNamed(nameof(FixFurniture)))
            })
            .Finish();

        private static bool FixFurniture(Furniture instance, ref SObject original)
        {
            if (instance.GetType() == original.GetType())
                return false;
            instance.currentRotation.Value = ((Furniture)original).currentRotation.Value;
            instance.updateRotation();
            original = instance;
            return true;
        }

        private static IEnumerable<CodeInstruction> replaceLocals(ILHelper.ILEnumerator cursor)
        {
            yield return cursor.source.Current;
            while (cursor.source.MoveNext())
            {
                var c = cursor.source.Current;
                if (c.opcode == OpCodes.Ldloc_S && ((LocalBuilder)c.operand).LocalIndex == 31)
                    c.operand = cursor.GetLocal("inst");
                yield return c;
            }
        }
    }
}
