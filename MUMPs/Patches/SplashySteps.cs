using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
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
            .Finish();
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => patcher.Run(instructions);
        private static string shouldUseSplash(Farmer who, FarmerSprite sprite)
        {
            var pos = who.getTileLocationPoint();
            return (who.currentLocation is not BoatTunnel && 
                who.currentLocation.doesTileHaveProperty(pos.X, pos.Y, "Water", "Back") != null && 
                who.currentLocation.getTileIndexAt(pos, "Buildings") == -1 &&
                !Game1.eventUp
                ) ? "quickSlosh" : sprite.currentStep;
        }
    }
}
