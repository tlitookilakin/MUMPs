﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
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
            } catch(FormatException)
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
        public static IEnumerable<CodeInstruction> InjectAt(
            CodeInstruction[] Anchors, 
            IEnumerable<CodeInstruction> instructions, 
            string Name, 
            Func<IEnumerable<CodeInstruction>> injection, 
            List<LocalBuilder> boxes)
        {
            ModEntry.monitor.Log("Now applying patch '" + Name + "'...", LogLevel.Debug);
            int marker = 0;
            foreach (var code in instructions)
            {
                if (marker > -1)
                {
                    if (marker >= Anchors.Length)
                    {
                        foreach (var inst in injection())
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
                            if (code.operand is LocalBuilder b && boxes != null)
                                boxes.Add(b);
                        }
                        else
                        {
                            boxes.Clear();
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
                return (oper2.Item1 < 0 || oper1.LocalIndex == oper2.Item1) && (oper2.Item2 == null || oper1.LocalType == oper2.Item2);
            }
            return false;
        }
        public static void AddAction(string Name, bool isInspect, Action<Farmer, string> action)
        {
            Patches.Action.actions.Add(Name, action);
            if (isInspect)
                Patches.Action.inspectActions.Add(Name);
        }
        public static IEnumerable<string> SafeSplit(string s, char delim)
        {
            bool dquote = false;
            bool squote = false;
            bool escaped = false;
            StringBuilder sb = new();
            foreach(char c in s)
            {
                if (escaped)
                {
                    escaped = false;
                    sb.Append(c);
                    continue;
                }
                switch (c)
                {
                    case '"':
                        if (!squote)
                        {
                            dquote = !dquote;
                            continue;
                        }
                        break;
                    case '\'':
                        if (!dquote)
                        {
                            squote = !squote;
                            continue;
                        }
                        break;
                    case '\\':
                        escaped = true;
                        continue;
                    default:
                        if (c == delim && !dquote && !squote)
                        {
                            if (sb.Length > 0)
                                yield return sb.ToString();
                            sb.Clear();
                            continue;
                        }
                        break;
                }
                sb.Append(c);
            }
            yield return sb.ToString();
        }
        public static List<string> SafeSplitList(string s, char delim)
        {
            var list = new List<string>();
            foreach(string item in SafeSplit(s, delim))
            {
                list.Add(item);
            }
            return list;
        }
        public static void warpToTempMap(string path, Farmer who)
        {
            GameLocation temp = new(PathUtilities.NormalizeAssetName("Maps/"+path), "Temp");
            temp.map.LoadTileSheets(Game1.mapDisplayDevice);
            Event e = Game1.currentLocation.currentEvent;
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation.currentEvent = null;
            Game1.currentLightSources.Clear();
            Game1.currentLocation = temp;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.currentLocation.currentEvent = e;
            Game1.player.currentLocation = Game1.currentLocation;
            who.currentLocation = Game1.currentLocation;
            Game1.panScreen(0, 0);
        }

        //Used to get DGA item #
        public static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
