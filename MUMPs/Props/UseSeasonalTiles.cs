using HarmonyLib;
using StardewValley;
using System;
using System.IO;
using System.Linq;

namespace MUMPs.Props
{
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.updateSeasonalTileSheets))]
    internal class UseSeasonalTiles
    {
        public static bool Prefix(GameLocation __instance, xTile.Map map)
        {
            map ??= __instance.Map;
            if (map is null)
                return false;
            if (!TryGetNames(map, out var names))
                return true;

            map.DisposeTileSheets(Game1.mapDisplayDevice);
            for (int i = 0; i < map.TileSheets.Count; i++)
                map.TileSheets[i].ImageSource = SetSeasonSheet(map.TileSheets[i].ImageSource, __instance.GetSeasonForLocation(), names);
            map.LoadTileSheets(Game1.mapDisplayDevice);

            return false;
        }

        private static string StripPathAndLocale(string from)
        {
            var s = Path.GetFileNameWithoutExtension(from);
            var locale = ModEntry.i18n.Locale;
            s = locale == "" || !s.EndsWith("." + locale) ? s : s[^locale.Length..];
            if (s.StartsWith("fall_"))
                s = s[5..];
            else if (s[0..7] is "spring_" or "summer_" or "winter_")
                s = s[7..];
            return s;
        }

        private static string SetSeasonSheet(string fname, string season, string[] split)
            => split is null || split.Length == 0 || split.Contains(StripPathAndLocale(fname)) ?
            GameLocation.GetSeasonalTilesheetName(fname, season) : fname;

        private static bool TryGetNames(xTile.Map map, out string[] split)
        {
            split = null;
            if (map is null || map.Properties is null || !map.Properties.TryGetValue("UseSeasonalTiles", out var prop))
                return false;
            if (prop is null)
                return true;
            split = prop.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return true;
        }
    }
}
