﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static IEnumerable<CodeInstruction> InjectAt(CodeInstruction[] Injection, CodeInstruction[] Anchors, IEnumerable<CodeInstruction> instructions)
        {
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
                    else if (code == Anchors[marker])
                    {
                        marker++;
                    }
                    else
                    {
                        marker = 0;
                    }
                }
                yield return code;
            }
            if (marker != -1)
                throw new FormatException("Marker instructions not found.");
        }
    }
}
