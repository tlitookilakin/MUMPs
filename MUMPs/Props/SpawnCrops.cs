using AeroCore;
using StardewModdingAPI.Events;
using StardewValley;
using AeroCore.Utils;
using System;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Objects;

namespace MUMPs.Props
{
    [ModInit]
    internal class SpawnCrops
    {
        internal static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayStarted += NewDay;
        }
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
            if (season == -1)
                season = Utility.getSeasonNumber(Game1.currentSeason);
            foreach ((var tile, var x, var y) in loc.Map.TilesInLayer("Back"))
            {
                if (AddGiantCrop(tile, x, y, loc, isNewSeason, season))
                    continue;
                if (!tile.TileHasProperty("Crop", out var prop))
                    continue;
                Vector2 pos = new(x, y);
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
                if (split.Length < 2)
                    continue;
                string id = split.Length > season + 1 ? split[season + 1] : split.Length > 2 ? string.Empty : split[1];

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
        private static bool AddGiantCrop(xTile.Tiles.Tile tile, int x, int y, GameLocation loc, bool newSeason, int season)
        {
            if (!tile.TileHasProperty("GiantCrop", out var prop))
                return false;
            var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                return false;
            var clump = loc.ResourceClumpIntersecting(x, y);
            if (clump is not null)
            {
                if (!newSeason)
                {
                    if (split[0].StartsWith("T", StringComparison.OrdinalIgnoreCase))
                        clump.modData.Remove("tlitookilakin.mumps.noInteract");
                    else
                        clump.modData["tlitookilakin.mumps.noInteract"] = "T";
                    return true;
                }
                loc.resourceClumps.Remove(clump);
            }
            string which = split.Length > season + 1 ? split[season + 1] : split.Length > 2 ? string.Empty : split[1];
            if (!int.TryParse(which, out var id))
                return false;
            for (int tx = 0; tx < 3; tx++)
                for (int ty = 0; ty < 3; ty++)
                    if (loc.terrainFeatures[new(tx + x, ty + y)] is HoeDirt dirt)
                        dirt.crop = null;
            var giant = new GiantCrop(id, new(x, y));
            if (!split[0].StartsWith("T", StringComparison.OrdinalIgnoreCase))
                giant.modData["tlitookilakin.mumps.noInteract"] = "T";
            loc.resourceClumps.Add(giant);
            return true;
        }
    }
}
