using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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
            ModEntry.OnChangeLocation += Generate;
        }
        private static void UpdateUsable(GameLocation loc)
        {
            foreach ((var tile, int x, int y) in loc.Map?.TilesInLayer("Back"))
            {
                Vector2 pos = new(x, y);
                if (tile.TileHasProperty("FruitTree", out string fprop) && loc.terrainFeatures[pos] is FruitTree tree)
                {
                    if(fprop.GetChunk(' ', 1).StartsWith("T", StringComparison.OrdinalIgnoreCase))
                        tree.modData.Remove("tlitookilakin.mumps.noInteract");
                    else
                        tree.modData["tlitookilakin.mumps.noInteract"] = "T";
                }
                var ef = loc.GetFurnitureAt(pos);
                if (tile.TileHasProperty("SpawnObject", out string oprop) && loc.Objects.TryGetValue(pos, out var eo))
                {
                    var s = oprop.GetChunk(' ', 1);
                    bool pickup = s.Equals("pickup", StringComparison.OrdinalIgnoreCase);
                    bool interact = pickup || s.Equals("use", StringComparison.OrdinalIgnoreCase); 
                    ModEntry.monitor.Log($"Updating usability of item @ ({pos.X}, {pos.Y}) in location '{loc.Name}'");
                    if (ef is not null)
                    {
                        if (pickup)
                            ef.modData.Remove("tlitookilakin.mumps.noPickup");
                        else
                            ef.modData["tlitookilakin.mumps.noPickup"] = "T";
                        if (interact)
                            ef.modData.Remove("tlitookilakin.mumps.noInteract");
                        else
                            ef.modData["tlitookilakin.mumps.noInteract"] = "T";
                    }
                    if (eo is not null)
                    {
                        if (pickup)
                            eo.modData.Remove("tlitookilakin.mumps.noPickup");
                        else
                            eo.modData["tlitookilakin.mumps.noPickup"] = "T";
                        if (interact)
                            eo.modData.Remove("tlitookilakin.mumps.noInteract");
                        else
                            eo.modData["tlitookilakin.mumps.noInteract"] = "T";
                    }
                }
            }
        }
        internal static void Generate(GameLocation loc)
        {
            UpdateUsable(loc);

            if (loc.modData.ContainsKey("tlitookilakin.mumps.generatedObjects"))
                return;
            loc.modData.Add("tlitookilakin.mumps.generatedObjects", "y");

            foreach ((var tile, int x, int y) in loc.Map?.TilesInLayer("Back"))
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
            bool pickup = split.Length > 1 && split[1].Equals("pickup", StringComparison.OrdinalIgnoreCase);
            bool interact = pickup || (split.Length > 1 && split[1].Equals("use", StringComparison.OrdinalIgnoreCase));
            if (!split[0].TryGetItem(out var item))
            {
                ModEntry.monitor.Log($"Could not spawn item '{id}' @ ({pos.X},{pos.Y}) in location '{loc.Name}'", LogLevel.Warn);
                return;
            }
            HoeDirt dirt = null;
            if (loc.terrainFeatures.TryGetValue(pos, out var tf))
                dirt = tf as HoeDirt;
            loc.terrainFeatures.Remove(pos);
            if (item is SObject obj)
            {
                if (split.Length > 2 && obj is Furniture furn && int.TryParse(split[2], out int rot))
                {
                    rot = furn.rotations.Value == 4 ? rot : rot * 2;
                    furn.currentRotation.Value = Math.Clamp(rot, 0, furn.rotations.Value - 1);
                    furn.updateRotation();
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
                    else if (obj is MiniJukebox juke && split.Length > 3)
                        juke.OnSongChosen(split[3]);
                    else if (obj is Chest chest)
                        for(int i = 3; i < split.Length; i++)
                            if (split[i].TryGetItem(out var si))
                                chest.items.Add(si);
                    if (!interact)
                        obj.modData["tlitookilakin.mumps.noInteract"] = "T";
                    if (!pickup)
                        obj.modData["tlitookilakin.mumps.noPickup"] = "T";
                    if (obj is Sign && split.Length > 3 && split[3].TryGetItem(out var disp))
                        obj.heldObject.Value = disp as SObject;
                } else if (loc.terrainFeatures.TryGetValue(pos, out tf) && !pickup)
                {
                    tf.modData["tlitookilakin.mumps.noInteract"] = "T";
                } else
                {
                    var f = loc.GetFurnitureAt(pos);
                    if (f is not null)
                    {
                        if (f is StorageFurniture sf)
                            for (int i = 3; i < split.Length; i++)
                                if (split[i].TryGetItem(out var si))
                                    sf.heldItems.Add(si);
                                else
                                    continue;
                        else if (split.Length > 3 && split[3].TryGetItem(out var si))
                            f.performObjectDropInAction(si, true, null);
                        // must be after drop-in, or it might get canceled
                        if (!interact)
                            f.modData["tlitookilakin.mumps.noInteract"] = "T";
                        if (!pickup)
                            f.modData["tlitookilakin.mumps.noPickup"] = "T";
                    }
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
