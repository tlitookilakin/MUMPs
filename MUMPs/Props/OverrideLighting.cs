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
    [HarmonyPatch]
    class OverrideLighting
    {
        public static readonly PerScreen<bool> force = new(() => false);
        private static readonly ILHelper patcher = setupHelper();
        public static void ChangeLocation(GameLocation loc)
        {
            force.Value = loc is not Woods && loc.getMapProperty("OverrideLighting") != null;
        }
        [HarmonyPatch(typeof(GameLocation),"_updateAmbientLighting")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static bool ShouldOverride(bool overrideNet)
        {
            return overrideNet || force.Value;
        }
        private static ILHelper setupHelper()
        {
            ILHelper helper = new("LightOverride");
            helper.SkipTo(new CodeInstruction[] {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld,typeof(GameLocation).FieldNamed("ignoreOutdoorLighting"))
            }).Skip().Add(new CodeInstruction[] {
                new(OpCodes.Call, typeof(OverrideLighting).MethodNamed("ShouldOverride"))
            }).Finish();
            return helper;
        }
        public static void Cleanup()
        {
            force.ResetAllScreens();
        }
    }
}
