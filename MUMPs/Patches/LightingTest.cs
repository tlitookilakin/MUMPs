using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MUMPs.Patches
{
    [HarmonyPatch]
    static class LightingTest
    {
        private static readonly CodeInstruction[] anchors = {
            new(OpCodes.Call, AccessTools.Method(typeof(Game1),"get_lightmap")),
            new(OpCodes.Callvirt, typeof(Texture2D).GetMethod("get_Bounds")),
            new(OpCodes.Ldloc_S, (23, typeof(Color))),
            new(OpCodes.Callvirt, typeof(SpriteBatch).GetMethod("Draw", new Type[]{typeof(Texture2D),typeof(Rectangle),typeof(Color)}))
        };
        private static readonly CodeInstruction[] injected = {
            new(OpCodes.Ldloc_S, 24),
            new(OpCodes.Call, typeof(LightingTest).GetMethod("RunLighting"))
        };
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("StardewModdingAPI.Framework.SGame"), "DrawImpl");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach(var code in Utils.InjectAt(injected, anchors, instructions, "Lighting Test"))
            {
                yield return code;
            }
        }
        public static void RunLighting(float intensity)
        {
            //noop
        }
    }
}
