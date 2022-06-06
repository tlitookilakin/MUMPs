using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
    [HarmonyPatch(typeof(GameLocation), "_updateAmbientLighting")]
    [ModInit]
    class OverrideLighting
    {
        private static readonly PerScreen<bool> force = new(() => false);
        internal static void ChangeLocation(GameLocation loc) => force.Value = loc is not Woods && loc.getMapProperty("OverrideLighting") != null;

        internal static void Init()
        {
            ModEntry.OnCleanup += Cleanup;
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => patcher.Run(instructions);
        private static readonly ILHelper patcher = new ILHelper(ModEntry.monitor, "Override Lighting")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld,typeof(GameLocation).FieldNamed("ignoreOutdoorLighting"))
            })
            .Skip(3)
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldsfld, typeof(OverrideLighting).FieldNamed(nameof(force))),
                new(OpCodes.Callvirt, typeof(PerScreen<bool>).PropertyGetter(nameof(PerScreen<bool>.Value))),
                new(OpCodes.Or)
            })
            .Finish();
        private static void Cleanup() => force.ResetAllScreens();
    }
}
