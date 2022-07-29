using AeroCore;
using StardewModdingAPI.Events;
using StardewValley;
using AeroCore.Utils;
using System;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using HarmonyLib;

namespace MUMPs.Props
{
    [ModInit]
    internal class SpawnCrops
    {
        internal static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayStarted += NewDay;
            HarmonyMethod patch = new(typeof(SpawnCrops).MethodNamed(nameof(NoHarvest)));
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.performToolAction)), patch);
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.performUseAction)), patch);
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.readyForHarvest)), patch);
        }
        private static bool NoHarvest(HoeDirt __instance, ref bool __result)
        {
            bool noHarvest = __instance.modData.ContainsKey("tlitookilakin.mumps.noHarvest");
            __result = !noHarvest && __result;
            return !noHarvest;
        }
        private static void NewDay(object _, DayStartedEventArgs ev)
        {
            bool newSeason = Game1.dayOfMonth == 1;
            if(Context.IsMainPlayer)
                foreach (var loc in Game1.locations)
                    Spawn(loc, newSeason);
        }
        private static void Spawn(GameLocation loc, bool isNewSeason)
        {
            int season = loc.GetSeasonIndexForLocation();
            foreach((var tile, var x, var y) in loc.Map.TilesInLayer("Back"))
            {
                if (!tile.TileHasProperty("SpawnCrop", out var prop))
                    continue;
                Vector2 pos = new(x, y);
                HoeDirt dirt;
                if (loc.terrainFeatures.TryGetValue(pos, out var tf))
                    if (tf is not HoeDirt)
                        continue;
                    else
                        dirt = tf as HoeDirt;
                else
                    loc.terrainFeatures.Add(pos, dirt = new(1, loc));
                if (dirt.crop is not null && dirt.crop.dead.Value)
                    dirt.destroyCrop(pos, false, loc);
                dirt.state.Value = 1; // water it

                var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string id;
                switch (split.Length)
                {
                    case 2: id = split[1]; break;
                    case 5: id = split[season + 1]; break;
                    default: continue;
                }
                if (id.TryGetCrop(x, y, out var crop) && (dirt.crop is null || 
                    (isNewSeason && dirt.crop.netSeedIndex.Value != crop.netSeedIndex.Value)))
                    dirt.crop = crop;
                if (split[0].ToUpperInvariant()[0] != 'T')
                    dirt.modData["tlitookilakin.mumps.noHarvest"] = "T";
                else
                    dirt.modData.Remove("tlitookilakin.mumps.noHarvest");
                dirt.paddyWaterCheck(loc, pos);
                dirt.updateNeighbors(loc, pos);
            }
        }
    }
}
