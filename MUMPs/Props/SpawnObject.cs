using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace MUMPs.Props
{
    [ModInit]
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.DayUpdate))]
    class SpawnObject
    {
        internal static void Init()
        {
            //ModEntry.helper.Events.GameLoop.DayStarted += DayUpdate;
            ModEntry.OnChangeLocation += Generate;
        }
        private static void DayUpdate(object _, DayStartedEventArgs ev)
        {
            if (!Context.IsMainPlayer)
                return;

            foreach (var loc in Game1.locations)
                Generate(loc);
        }
        internal static void Generate(GameLocation loc)
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
            if (loc.Objects.ContainsKey(pos) || loc.GetFurnitureAt(pos) is not null)
                return;
            string[] split = id.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return;
            if (!split[0].TryGetItem(out var item))
            {
                ModEntry.monitor.Log($"Could not spawn item '{id}' @ ({pos.X},{pos.Y}) in location '{loc.Name}'", LogLevel.Warn);
                return;
            }
            HoeDirt dirt = null;
            if (loc.terrainFeatures.TryGetValue(pos, out var tf))
                dirt = tf as HoeDirt;
            bool interact = split.Length > 1 && split[1].StartsWith("T", StringComparison.OrdinalIgnoreCase);
            loc.terrainFeatures.Remove(pos);
            if (item is SObject obj)
            {
                if (split.Length > 2 && obj is Furniture furn && int.TryParse(split[2], out int rot))
                {
                    rot = furn.rotations.Value == 4 ? rot : rot * 2;
                    furn.currentRotation.Value = Math.Clamp(rot, 0, furn.rotations.Value - 1);
                    furn.updateRotation();
                    if (!interact)
                        furn.modData["tlitookilakin.mumps.noInteract"] = "T";
                }
                if (obj.isSapling() || obj.Category is -74 or -19 || 
                    !obj.placementAction(loc, (int)pos.X * 64, (int)pos.Y * 64, Game1.player))
                {
                    loc.objects[pos] = obj;
                    obj.TileLocation = pos;
                }
                if (loc.Objects.TryGetValue(pos, out obj) && obj is not null)
                {
                    if (!obj.bigCraftable.Value)
                    {
                        obj.CanBeGrabbed = true;
                        obj.IsSpawnedObject = true;
                        obj.modData["tlitookilakin.mumps.persist"] = "T";
                    }
                    if (obj is IndoorPot pot && dirt is not null)
                        pot.hoeDirt.Value = dirt;
                    if (!interact)
                        obj.modData["tlitookilakin.mumps.noInteract"] = "T";
                } else if (loc.terrainFeatures.TryGetValue(pos, out tf) && !interact)
                {
                    tf.modData["tlitookilakin.mumps.noInteract"] = "T";
                }
                return;
            }
            // interact field is ignored for unplaceable items
            if (split.Length > 2 && int.TryParse(split[2], out int ind) && ind >= 0)
                loc.objects[pos] = new Chest(0, new() { item }, pos, true, ind);
            else
                loc.objects[pos] = new Chest(0, new() { item }, pos);
        }
        internal static void AddFruitTree(GameLocation loc, Vector2 pos, string str)
        {
            if (loc.terrainFeatures.ContainsKey(pos))
                return;
            var split = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0 || !split[0].TryGetFruitTree(out var tree, 4))
                return;
            tree.daysUntilMature.Value = 0;
            if (split.Length > 1 && !split[1].StartsWith("T", StringComparison.OrdinalIgnoreCase))
                tree.modData["tlitookilakin.mumps.noInteract"] = "T";
            loc.terrainFeatures[pos] = tree;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> PersistPatch(IEnumerable<CodeInstruction> codes, ILGenerator gen)
            => patcher.Run(codes, gen);

        private static readonly ILHelper patcher = new ILHelper(ModEntry.monitor, "spawnable persistance")
            .SkipTo(new CodeInstruction(OpCodes.Ldfld, typeof(SObject).FieldNamed(nameof(SObject.isSpawnedObject))))
            .Skip(2)
            .Transform(addCheck)
            .Remove(1)
            .Finish();

        private static IList<CodeInstruction> addCheck(ILHelper.ILEnumerator cursor)
            => new[]
            {
                cursor.Current,
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_S, 18),
                new(OpCodes.Call, typeof(SpawnObject).MethodNamed(nameof(check))),
                new(OpCodes.Brtrue_S, cursor.Current.operand)
            };

        private static bool check(GameLocation loc, int index)
            => loc.Objects.Pairs.ElementAt(index).Value.modData.ContainsKey("tlitookilakin.mumps.persist");
    }
}
