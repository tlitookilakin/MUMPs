using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MUMPs.Patches
{
    [HarmonyPatch(typeof(FarmerSprite),"checkForFootstep")]
    class SplashySteps
    {
        private static ILHelper patcher = setupPatcher();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return patcher.Run(instructions);
        }
        private static ILHelper setupPatcher()
        {
            ILHelper helper = new("Splashy Steps");
            helper.SkipTo(new CodeInstruction[]{
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(FarmerSprite).GetField("currentStep")),
                new(OpCodes.Stloc_2)
            }).Add(new CodeInstruction[]{
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(FarmerSprite).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(SplashySteps).GetMethod("shouldUseSplash")),
                new(OpCodes.Stloc_2)
            }).Finish();
            return helper;
        }
        public static string shouldUseSplash(Farmer who, FarmerSprite sprite)
        {
            var pos = who.getTileLocationPoint();
            return (who.currentLocation.doesTileHaveProperty(pos.X, pos.Y, "Water", "Back") != null && who.currentLocation.getTileIndexAt(pos, "Buildings") == -1) ? "slosh" : sprite.currentStep;
        }
    }
}
