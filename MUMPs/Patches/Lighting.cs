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
    static class Lighting
    {
        public static event LightingEventHandler OnLighting;
        public delegate void LightingEventHandler(object sender, float intensity);
        private static ILHelper patcher = setupPatch();
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("StardewModdingAPI.Framework.SGame"), "DrawImpl");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return patcher.Run(instructions);
        }
        private static ILHelper setupPatch()
        {
            ILHelper helper = new("Lighting");
            helper.SkipTo(new CodeInstruction[]{
                new(OpCodes.Call, AccessTools.Method(typeof(Game1),"get_lightmap")),
                new(OpCodes.Callvirt, typeof(Texture2D).GetMethod("get_Bounds")),
                new(OpCodes.Ldloc_S, (-1, typeof(Color))),
                new(OpCodes.Callvirt, typeof(SpriteBatch).GetMethod("Draw", new Type[]{typeof(Texture2D),typeof(Rectangle),typeof(Color)}))
            }).Add(Inject).Finish();
            return helper;
        }
        private static IEnumerable<CodeInstruction> Inject(IList<LocalBuilder> boxes)
        {
            yield return new(OpCodes.Ldloc_S, boxes[0].LocalIndex + 1);
            yield return new(OpCodes.Call, typeof(Lighting).GetMethod("RunLighting"));
        }
        public static void RunLighting(float intensity)
        {
            OnLighting?.Invoke(null, intensity);
        }
    }
}
