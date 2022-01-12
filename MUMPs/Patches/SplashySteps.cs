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
        internal static readonly CodeInstruction[] markers = {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, typeof(FarmerSprite).GetField("currentStep")),
            new(OpCodes.Stloc_2)
        };
        internal static readonly CodeInstruction[] inject =
        {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, typeof(FarmerSprite).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, typeof(SplashySteps).GetMethod("shouldUseSplash")),
            new(OpCodes.Stloc_2)
        };
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in Utils.InjectAt(markers, instructions, "Splashy Steps", Inject, null))
            {
                yield return code;
            }
        }
        public static IEnumerable<CodeInstruction> Inject()
        {
            foreach(var code in inject)
            {
                yield return code;
            }
        }
        public static string shouldUseSplash(Farmer who, FarmerSprite sprite)
        {
            var pos = who.getTileLocationPoint();
            return (who.currentLocation.doesTileHaveProperty(pos.X, pos.Y, "Water", "Back") != null && who.currentLocation.getTileIndexAt(pos, "Buildings") == -1) ? "slosh" : sprite.currentStep;
        }
    }
}
