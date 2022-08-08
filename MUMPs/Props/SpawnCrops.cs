using AeroCore;
using StardewModdingAPI.Events;
using StardewValley;
using AeroCore.Utils;
using System;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using HarmonyLib;
using StardewValley.Objects;

namespace MUMPs.Props
{
    [ModInit]
    internal class SpawnCrops
    {
        private static void NewDay(object _, DayStartedEventArgs ev)
        {
            bool newSeason = Game1.dayOfMonth == 1;
            if(Context.IsMainPlayer)
                foreach (var loc in Game1.locations)
                    Spawn(loc, newSeason);
        }
        internal static void Spawn(GameLocation loc, bool isNewSeason)
        {
            int season = loc.GetSeasonIndexForLocation();
            foreach((var tile, var x, var y) in loc.Map.TilesInLayer("Back"))
            {
                if (!tile.TileHasProperty("Crop", out var prop))
                    continue;
                Vector2 pos = new(x, y);
                if (loc.ResourceClumpAt(pos) is not null)
                    continue;
                HoeDirt dirt;
                if (loc.terrainFeatures.TryGetValue(pos, out var tf))
                    if (tf is not HoeDirt)
                        continue;
                    else
                        dirt = tf as HoeDirt;
                else if (loc.Objects.TryGetValue(pos, out var sobj))
                    if (sobj is IndoorPot pot)
                        dirt = pot.hoeDirt.Value;
                    else
                        continue;
                else
                    loc.terrainFeatures.Add(pos, dirt = new(1, loc));
                if (dirt.crop is not null && dirt.crop.dead.Value)
                    dirt.destroyCrop(pos, false, loc);
                dirt.state.Value = 1; // water it

                var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (season == -1)
                    season = StardewValley.Utility.getSeasonNumber(Game1.currentSeason);
                string id;
                if (split.Length == 2)
                    id = split[1];
                else if (season + 1 < split.Length)
                    id = split[season + 1];
                else
                    continue;

                // if the crop is valid, and either the dirt is empty or it's a new season with different crops
                // will not work with forage crops

                if (id.TryGetCrop(x, y, out var crop) && (dirt.crop is null || 
                    (isNewSeason && dirt.crop.netSeedIndex.Value != crop.netSeedIndex.Value)))
                    dirt.crop = crop;
                if (!split[0].StartsWith("T", StringComparison.OrdinalIgnoreCase))
                    dirt.modData["tlitookilakin.mumps.noInteract"] = "T";
                else
                    dirt.modData.Remove("tlitookilakin.mumps.noInteract");
                dirt.paddyWaterCheck(loc, pos);
                dirt.updateNeighbors(loc, pos);
            }
        }
    }
}
