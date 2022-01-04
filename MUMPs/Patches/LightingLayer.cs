using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using xTile;
using StardewModdingAPI;
using System.Reflection;

namespace MUMPs.Patches
{
    [HarmonyPatch]
    public static class LightingLayer
    {
        private static models.LightingDevice displayDevice = new();
        private static readonly CodeInstruction[] anchors = {
            CodeInstruction.Call(typeof(Game1),"get_lightmap"),
            CodeInstruction.Call(typeof(Texture2D),"get_Bounds"),
            new(OpCodes.Ldloc_S, 23),
            CodeInstruction.Call(typeof(SpriteBatch),"Draw",new Type[]{typeof(Texture2D),typeof(Rectangle),typeof(Color)})
        };
        private static readonly CodeInstruction[] injected = { 
            new(OpCodes.Ldloc_S, 24),
            CodeInstruction.Call(typeof(LightingLayer),"DrawLightingLayer")
        };
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("StardewModdingAPI.Framework.SGame");
            return AccessTools.Method(type, "DrawImpl");
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in Utils.InjectAt(injected, anchors, instructions))
            {
                yield return code;
            }
        }
        public static void DrawLightingLayer(float multiplier)
        {
            Map map = Game1.currentLocation?.Map;

            if (map == null)
                return;
            displayDevice.Multiplier = multiplier;
            displayDevice.BeginScene(Game1.spriteBatch);
            map.GetLayer("Lighting")?.Draw(displayDevice, Game1.viewport, new(), false, 3);
        }
    }
}
