using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.Dimensions;
using xTile.Display;

namespace MUMPs.Patches
{
    [HarmonyPatch(typeof(Game1))]
    [HarmonyPatch("_draw")]
    public static class LightingLayer
    {
        private static models.LightingDevice displayDevice = new();
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int marker = 0;
            object batchop = null;
            bool opped = false;
            foreach(var code in instructions)
            {
                if (opped)
                {
                    yield return code;
                    continue;
                }
                if(marker == 0 && code.opcode == OpCodes.Call && code.Calls(typeof(Game1).GetMethod("get_lightmap")))
                {
                    marker++;
                } else if(code.opcode == OpCodes.Callvirt)
                {
                    if(marker == 1 && code.Calls(typeof(Texture2D).GetMethod("get_bounds"))){
                        marker++;
                    } else if(marker == 3 && code.Calls(typeof(SpriteBatch).GetMethod("Draw")))
                    {
                        marker++;
                    }
                    else
                    {
                        marker = 0;
                    }
                } else if(marker == 2 && code.opcode == OpCodes.Ldloc_S)
                {
                    batchop = code.operand;
                    marker++;
                } else if(marker == 4)
                {
                    yield return new(OpCodes.Ldloc_S, batchop);
                    yield return CodeInstruction.Call(typeof(LightingLayer), "DrawLightingLayer");
                    marker = 0;
                    opped = true;
                    yield return code;
                } else
                {
                    marker = 0;
                    yield return code;
                }
            }
        }
        private static void DrawLightingLayer(SpriteBatch b)
        {
            Map map = Game1.currentLocation?.Map;

            if (map == null)
                return;
            displayDevice.BeginScene(b);
            map.GetLayer("Lighting")?.Draw(displayDevice, Game1.viewport, new(), false, 3);
        }
    }
}
