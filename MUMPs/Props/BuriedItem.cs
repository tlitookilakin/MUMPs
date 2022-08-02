using AeroCore.Utils;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

namespace MUMPs.Props
{
    [HarmonyPatch]
    internal class BuriedItem
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkForBuriedItem))]
        [HarmonyPrefix]
        private static bool CheckHere(int xLocation, int yLocation, bool detectOnly, GameLocation __instance, Farmer who)
        {
            if (detectOnly)
                return true;
            var tile = __instance.map?.GetLayer("Back").Tiles[new(xLocation, yLocation)];
            if (tile is null)
                return true;
            if (!tile.TileHasProperty("Treasure", out var prop) || prop is null)
                return true;
            var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0 || !(
                split[0].Equals("Arch", StringComparison.OrdinalIgnoreCase) || split[0].Equals("Object", StringComparison.OrdinalIgnoreCase)
                ) || int.TryParse(split[1], out _))
                return true;

            if (split[1].TryGetItem(out var item))
                Game1.createItemDebris(item, new(xLocation * 64f, yLocation * 64f), Game1.random.Next(0, 4), __instance);
            else
                ModEntry.monitor.Log(
                    $"Could not spawn diggable item '{split[1]}' @ [{xLocation}, {yLocation}] in '{__instance.Name}'.", 
                LogLevel.Warn);
            tile.Properties["Treasure"] = null;

            return false;
        }
    }
}
