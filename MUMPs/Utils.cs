using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using xTile.Layers;

namespace MUMPs
{
    static class Utils
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
        public static MethodInfo MethodNamed(this Type type, string methodName, Type[] param)
        {
            return AccessTools.Method(type, methodName, param);
        }
        public static MethodInfo MethodNamed(this Type type, string MethodName)
        {
            return AccessTools.Method(type, MethodName);
        }
        public static FieldInfo FieldNamed(this Type type, string fieldName)
        {
            return AccessTools.Field(type, fieldName);
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
        public static IEnumerable<(xTile.Tiles.Tile, int, int)> tilesInLayer(Layer layer)
        {
            for(int x = 0; x < layer.LayerWidth; x++)
            {
                for(int y = 0; y < layer.LayerHeight; y++)
                {
                    var tile = layer.Tiles[x, y];
                    if(tile != null)
                    {
                        yield return (tile, x, y);
                    }
                }
            }
        }
        public static Point LocalToGlobal(int x, int y)
        {
            return new(x + Game1.viewport.X, y + Game1.viewport.Y);
        }
        public static void warpToTempMap(string path, Farmer who)
        {
            GameLocation temp = new(PathUtilities.NormalizeAssetName("Maps/"+path), "Temp");
            temp.map.LoadTileSheets(Game1.mapDisplayDevice);
            if(path.Trim() == "EventVoid")
                Events.drawVoid.Value = true; //anti-flicker
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
