using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace MUMPs.Props
{
    [ModInit]
    class SpawnObject
    {

        internal static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayStarted += DayUpdate;
        }
        private static void DayUpdate(object _, DayStartedEventArgs ev)
        {
            if (!Context.IsMainPlayer)
                return;

            foreach (var loc in Game1.locations)
                Generate(loc);
        }
        private static void Generate(GameLocation loc)
        {
            if (loc.modData.ContainsKey("tlitookilakin.mumps.generatedObjects"))
                return;
            loc.modData.Add("tlitookilakin.mumps.generatedObjects", "y");

            foreach ((var tile, int x, int y) in Maps.TilesInLayer(loc.map, "Back"))
            {
                if (tile.TileHasProperty("FruitTree", out string tree))
                    AddFruitTree(loc, new(x, y), tree);
                if (tile.TileHasProperty("SpawnObject", out string prop))
                    GenerateAt(loc, new(x, y), prop);
            }
        }
        internal static void GenerateAt(GameLocation loc, Vector2 pos, string id)
        {
            string[] split = id.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return;
            if (!split[0].TryGetItem(out var item))
                ModEntry.monitor.Log($"Could not spawn item '{id}' @ ({pos.X},{pos.Y}) in location '{loc.Name}'", LogLevel.Warn);
            if (item is SObject obj)
            {
                if (obj.bigCraftable.Value || obj is Furniture)
                    obj.placementAction(loc, (int)pos.X, (int)pos.Y);
                else
                    loc.objects[pos] = obj;
                if (obj is Furniture furn && split.Length > 1 && int.TryParse(split[1], out int rot))
                {
                    rot = furn.rotations.Value == 4 ? rot : rot * 2;
                    furn.currentRotation.Value = Math.Clamp(rot, 0, furn.rotations.Value - 1);
                    furn.updateRotation();
                }
                return;
            }
            if (split.Length > 1 && int.TryParse(split[1], out int ind) && ind >= 0)
                loc.objects[pos] = new Chest(0, new() { item }, pos, true, ind);
            else
                loc.objects[pos] = new Chest(0, new() { item }, pos);
        }
        internal static void AddFruitTree(GameLocation loc, Vector2 pos, string str)
        {
            if (!int.TryParse(str, out int id))
                return;

            FruitTree tree = new(id, 4);
            tree.daysUntilMature.Value = 0;
            loc.terrainFeatures[pos] = tree;
        }
    }
}
