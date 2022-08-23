using AeroCore.Generics;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MUMPs.models;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SObject = StardewValley.Object;

namespace MUMPs.Props
{
    [HarmonyPatch]
    internal class Forage
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.spawnObjects))]
        [HarmonyPrefix]
        internal static bool spawnForage(GameLocation __instance)
        {
            var defval = __instance.getMapProperty("DefaultForage");
            var defsplit = defval?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            bool skip = defsplit.Length > 2 && defsplit[2].StartsWith("T", StringComparison.OrdinalIgnoreCase);
            if (defsplit.Length > 1 && int.TryParse(defsplit[1], out int count))
            {
                var size = __instance.Map.GetLayer("Back").LayerSize;
                var forage = GetForageDataAt(defsplit[0], __instance);
                if (forage is null)
                    ModEntry.monitor.Log($"No forage data found for '{defsplit[0]}' in location '{__instance.Name}'.");
                else
                    SpawnInLocationArea(__instance, new(0, 0, size.Width, size.Height), count, forage);
            }
            SpawnInLocation(__instance);
            return skip;
        }
        internal static void SpawnInLocation(GameLocation loc)
        {
            if (loc is null)
                return;
            var prop = loc.getMapProperty("ForageAreaCorners");
            if (prop is null)
                return;
            var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < split.Length - 5; i += 6)
            {
                if (!split.FromCorners(out var rect, i) || !int.TryParse(split[i + 5], out int count))
                    continue;
                string current = split[i + 4];
                var forage = GetForageDataAt(current, loc);
                if (forage is null)
                    ModEntry.monitor.Log($"No forage data found for '{current}' in location '{loc.Name}'.");
                else
                    SpawnInLocationArea(loc, rect, count, forage);
            }
        }
        private static ForageData GetForageDataAt(string initial, GameLocation where)
        {
            List<string> history = new();
            ForageData forage = null;
            bool stop = false;
            while (!stop && Assets.Forage.TryGetValue(initial, out forage))
            {
                stop = true;
                foreach ((var over, var condition) in forage.Overrides)
                {
                    if (ModEntry.AeroAPI.CheckConditions(condition, target_location: where))
                    {
                        initial = over;
                        stop = false;
                        break;
                    }
                }
                if (history.Contains(initial))
                {
                    StringBuilder sb = new();
                    sb.Append("Cyclic forage table overrides encountered in location '");
                    sb.Append(where.Name).AppendLine("' @ path:");
                    for (int ii = 0; ii < history.Count; ii++)
                        sb.Append(history[ii]).Append(" > ");
                    sb.Append(initial);
                    ModEntry.monitor.Log(sb.ToString(), LogLevel.Warn);
                    break;
                }
            }
            return forage;
        }
        private static void SpawnInLocationArea(GameLocation loc, Rectangle region, int attempts, ForageData forage)
        {
            var pools = forage.GetForage();
            for(int i = 0; i < attempts; i++)
            {
                Vector2 tile = new(Game1.random.Next(region.Left, region.Right), Game1.random.Next(region.Top, region.Bottom));
                if (!CanSpawnAt(loc, tile))
                    continue;
                var ground = loc.doesTileHaveProperty((int)tile.X, (int)tile.Y, "type", "back");
                WeightedArray<SObject> pool;
                if (ground is null || ground == string.Empty)
                    pool = pools.Values.ElementAt(Game1.random.Next(pools.Count));
                else if (!pools.TryGetValue(ground, out pool))
                    continue;
                var obj = pool.Choose().getOne() as SObject;
                obj.IsSpawnedObject = true;
                obj.CanBeGrabbed = true;
                loc.Objects[new(tile.X, tile.Y)] = obj;
            }
        }
        internal static bool CanSpawnAt(GameLocation loc, Vector2 pos)
            => !loc.Objects.TryGetValue(pos, out _) && !loc.doesEitherTileOrTileIndexPropertyEqual((int)pos.X, (int)pos.Y, "Spawnable", "Back", "F") &&
            loc.isTileLocationTotallyClearAndPlaceable(pos) && loc.getTileIndexAt((int)pos.X, (int)pos.Y, "AlwaysFront") == -1 && 
            loc.getTileIndexAt((int)pos.X, (int)pos.Y, "Front") == -1 && !loc.isBehindBush(pos) && !loc.isBehindTree(pos);
    }
}
