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
        private static List<LocalBuilder> boxes = new();
        private static readonly CodeInstruction[] anchors = {
            new(OpCodes.Call, AccessTools.Method(typeof(Game1),"get_lightmap")),
            new(OpCodes.Callvirt, typeof(Texture2D).GetMethod("get_Bounds")),
            new(OpCodes.Ldloc_S, (-1, typeof(Color))),
            new(OpCodes.Callvirt, typeof(SpriteBatch).GetMethod("Draw", new Type[]{typeof(Texture2D),typeof(Rectangle),typeof(Color)}))
        };
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("StardewModdingAPI.Framework.SGame"), "DrawImpl");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            boxes.Clear();
            foreach(var code in Utils.InjectAt(anchors, instructions, "Lighting", Inject, boxes))
            {
                yield return code;
            }
        }
        public static IEnumerable<CodeInstruction> Inject()
        {
            yield return new(OpCodes.Ldloc_S, boxes[0].LocalIndex + 1);
            yield return new(OpCodes.Call, typeof(Lighting).GetMethod("RunLighting"));
        }
        public static void RunLighting(float intensity)
        {
            Props.LightingLayer.Draw(intensity);
        }
    }
}
