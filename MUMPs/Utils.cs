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
        internal static IReflectedField<Multiplayer> mpField = null;
        internal static void Setup()
        {
            mpField = ModEntry.helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
        }
        public static int ToInt(this float f)
        {
            return (int)MathF.Round(f);
        }
        public static bool TryParseColor(string s, out Color color)
        {
            color = Color.White;
            if (s.Length == 0)
            {
                ModEntry.monitor.Log("Could not parse color from string: '" + s + "'.", LogLevel.Warn);
                return false;
            }
            if (s[0] == '#')
            {
                if (s.Length <= 6)
                {
                    ModEntry.monitor.Log("Could not parse color from string: '" + s + "'.", LogLevel.Warn);
                    return false;
                }
                int r = Convert.ToInt32(s.Substring(1, 2), 16);
                int g = Convert.ToInt32(s.Substring(3, 2), 16);
                int b = Convert.ToInt32(s.Substring(5, 2), 16);
                if (s.Length > 8)
                {
                    int a = Convert.ToInt32(s.Substring(7, 2), 16);
                    color = new(r, g, b, a);
                    return true;
                }
                color =  new(r, g, b);
                return true;
            }
            else
            {
                string[] vals = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (vals.Length > 2)
                {
                    color = (vals.Length > 3) ?
                        new Color(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]), int.Parse(vals[3])) :
                        new Color(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]));
                    return true;
                }
                ModEntry.monitor.Log("Could not parse color from string: '" + s + "'.", LogLevel.Warn);
                return false;
            }
        }
        public static bool StringsToPoint(this string[] strings, out Point point, int offset = 0)
        {
            if(offset + 1 >= strings.Length)
            {
                point = new();
                return false;
            }
            return StringsToPoint(strings[offset], strings[offset + 1], out point);
        }
        public static bool StringsToPoint(string x, string y, out Point point)
        {
            if(int.TryParse(x, out int xx) && int.TryParse(y, out int yy))
            {
                point = new(xx, yy);
                return true;
            }
            point = new();
            return false;
        }
        public static bool StringsToVec2(this string[] strings, out Vector2 vec, int offset = 0)
        {
            if(offset + 1 >= strings.Length)
            {
                vec = new();
                return false;
            }
            return StringsToVec2(strings[offset], strings[offset + 1], out vec);
        }
        public static bool StringsToVec2(string x, string y, out Vector2 vec)
        {
            if(float.TryParse(x, out float xx) && float.TryParse(y, out float yy))
            {
                vec = new(xx, yy);
                return true;
            }
            vec = new();
            return false;
        }
        public static bool StringsToRect(this string[] strings, out Rectangle rect, int offset = 0)
        {
            if(offset + 3 >= strings.Length)
            {
                rect = new();
                return false;
            }
            return StringsToRect(strings[offset], strings[offset + 1], strings[offset + 2], strings[offset + 3], out rect);
        }
        public static bool StringsToRect(string x, string y, string w, string h, out Rectangle rect)
        {
            if(int.TryParse(x, out int xx) && int.TryParse(y, out int yy) && int.TryParse(w, out int ww) && int.TryParse(h, out int hh))
            {
                rect = new(xx, yy, ww, hh);
                return true;
            }
            rect = new();
            return false;
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
        public static void AddAction(string Name, bool isInspect, Action<Farmer, string, Point> action)
        {
            Patches.Action.actions.Add(Name, action);
            if (isInspect)
                Patches.Action.inspectActions.Add(Name);
        }
        public static IEnumerable<string> SafeSplit(this string s, char delim)
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
        public static List<string> SafeSplitList(this string s, char delim)
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
            if (layer == null)
                yield break;

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
        public static IEnumerable<(xTile.Tiles.Tile, int, int)> tilesInLayer(xTile.Map map, string layerName)
        {
            foreach (var item in tilesInLayer(map.GetLayer(layerName)))
                yield return item;
        }
        public static bool TileHasProperty(this xTile.Tiles.Tile tile, string name, out string prop)
        {
            bool ret = tile.Properties.TryGetValue(name, out var val) || tile.TileIndexProperties.TryGetValue(name, out val);
            prop = val?.ToString();
            return ret;
        }
        public static Point LocalToGlobal(int x, int y)
        {
            return new(x + Game1.viewport.X, y + Game1.viewport.Y);
        }
        public static Point LocalToGlobal(Point pos)
        {
            return LocalToGlobal(pos.X, pos.Y);
        }
        public static Multiplayer GetMultiplayer()
        {
            return mpField.GetValue();
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
