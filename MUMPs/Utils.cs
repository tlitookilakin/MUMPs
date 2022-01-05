using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;

namespace MUMPs
{
    class Utils
    {
        public static Point StringsToPoint(string x, string y)
        {
            try
            {
                return new Point(int.Parse(x), int.Parse(y));
            } catch(FormatException e)
            {
                ModEntry.monitor.Log("Bad property format: [" + x + ", " + y + "].", LogLevel.Warn);
                return new Point(0, 0);
            }
        }
        public static Vector2 StringsToVec2(string x, string y)
        {
            try
            {
                return new Vector2(float.Parse(x), float.Parse(y));
            } catch(FormatException e)
            {
                ModEntry.monitor.Log("Bad property format: [" + x + ", " + y + "].", LogLevel.Warn);
                return new Vector2(0f, 0f);
            }
        }
        public static string[] MapPropertyArray(GameLocation loc, string prop)
        {
            return loc.getMapProperty(prop).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        public static IEnumerable<CodeInstruction> InjectAt(CodeInstruction[] Injection, CodeInstruction[] Anchors, IEnumerable<CodeInstruction> instructions, string Name)
        {
            ModEntry.monitor.Log("Now applying patch '" + Name + "'...", LogLevel.Debug);
            int marker = 0;
            foreach (var code in instructions)
            {
                if (marker > -1)
                {
                    if (marker >= Anchors.Length)
                    {
                        foreach (var inst in Injection)
                        {
                            yield return inst;
                            marker = -1;
                        }
                    }
                    else
                    {
                        var s = Anchors[marker];
                        if (code.opcode == s.opcode && (code.operand == s.operand || CompareOperands(code.operand, s.operand)))
                        {
                            marker++;
                        }
                        else
                        {
                            marker = 0;
                        }
                    }
                }
                yield return code;
            }
            if (marker != -1)
                ModEntry.monitor.Log("Failed to apply patch '" + Name + "'; Marker instructions not found!", LogLevel.Error);
            else
                ModEntry.monitor.Log("Sucessfully applied patch '" + Name + "'.", LogLevel.Debug);
        }
        public static bool CompareOperands(object op1, object op2)
        {
            if (op1 is LocalBuilder oper1 && op2 is ValueTuple<int, Type> oper2)
            {
                return oper1.LocalIndex == oper2.Item1 && oper1.LocalType == oper2.Item2;
            }
            return false;
        }
    }
}
