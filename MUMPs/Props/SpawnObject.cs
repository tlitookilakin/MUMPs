using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    class SpawnObject
    {
        private static Dictionary<string, Action<GameLocation, Vector2, string>> generators = new() {
            {"item", AddObject},
            {"furniture", AddFurniture},
            {"bigcraftable",AddBigCraftable},
            {"fruittree",AddFruitTree}
        };
        internal static HashSet<int> TVIDs = new() {
            1466, 1468, 1680, 2326
        };
        private static Dictionary<int, string> furnitureData;

        public static void ChangeLocation(GameLocation loc)
        {
            furnitureData = ModEntry.helper.Content.Load<Dictionary<int, string>>("Data/Furniture", ContentSource.GameContent);

            if (loc.modData.ContainsKey("tlitookilakin.mumps.generatedObjects"))
                return;

            loc.modData.Add("tlitookilakin.mumps.generatedObjects", "y");

            Generate(loc);
        }
        public static void Generate(GameLocation loc)
        {
            foreach ((var tile, int x, int y) in Maps.tilesInLayer(loc.map, "Back"))
            {
                if (tile.TileHasProperty("SpawnObject", out string prop))
                {
                    string[] props = prop.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (props.Length > 1)
                        GenerateAt(loc, new(x, y), props[0], props[1]);
                }
            }
        }
        public static void GenerateAt(GameLocation loc, Vector2 pos, string type, string item)
        {
            if (generators.TryGetValue(type.ToLowerInvariant(), out var gen))
                gen(loc, pos, item);
            else
                ModEntry.monitor.Log("Could not spawn object type: '" + type + "'.", LogLevel.Warn);
        }
        public static void AddFurniture(GameLocation loc, Vector2 pos, string str)
        {
            string[] data = str.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!int.TryParse(data[0], out int id))
                return;

            if (data.Length < 2 || !int.TryParse(data[1], out int rot))
                rot = -1;
            loc.furniture.Add(CreateFurniture(id, pos, rot));
        }
        public static Furniture CreateFurniture(int id, Vector2 pos, int rot = -1)
        {
            if (TVIDs.Contains(id))
                return new TV(id, pos);
            else if(furnitureData.TryGetValue(id,out string data))
                switch (data.Split('/')[1].Split(' ')[0])
                {
                    case "bed":
                        return (rot >= 0) ? new BedFurniture(id, pos, rot) : new BedFurniture(id, pos);
                    case "fishtank":
                        return (rot >= 0) ? new FishTankFurniture(id, pos, rot) : new FishTankFurniture(id, pos);
                }
            return (rot >= 0) ? new Furniture(id, pos, rot) : new Furniture(id, pos);
        }
        public static void AddObject(GameLocation loc, Vector2 pos, string str)
        {
            if (!int.TryParse(str, out int id))
                return;

            StardewValley.Object obj;
            if (id == 93 || id == 94) //torch fix
                obj = new Torch(pos, 1, id);
            else
                obj = new(pos, id, null, false, true, false, true);
            loc.dropObject(obj, pos * 64f, Game1.viewport, true);
        }
        public static void AddBigCraftable(GameLocation loc, Vector2 pos, string str)
        {
            if (!int.TryParse(str, out int id))
                return;
            StardewValley.Object obj;
            switch (id)
            {
                case 62:
                    obj = new IndoorPot(pos); break;
                case 37:
                case 38:
                case 39:
                    obj = new Sign(pos, id); break;
                case 130:
                case 232:
                    obj = new Chest(true, pos, id); break;
                case 165:
                    obj = new(pos, id);
                    obj.heldObject.Value = new Chest();
                    break;
                case 163:
                    obj = new Cask(pos); break;
                case 208:
                    obj = new Workbench(pos); break;
                case 209:
                    obj = new MiniJukebox(pos);
                    loc.objects[pos] = obj;
                    (obj as MiniJukebox).RegisterToLocation(loc);
                    return;
                case 211:
                    obj = new WoodChipper(pos);
                    obj.placementAction(loc, (int)pos.X, (int)pos.Y);
                    break;
                case 214:
                    obj = new Phone(pos); break;
                case 216:
                    obj = new Chest(id, pos, 217, 2);
                    (obj as Chest).fridge.Value = true;
                    break;
                case 105:
                case 264:
                    if (loc.terrainFeatures[pos] is Tree tree)
                    {
                        obj = new(id, 1);
                        obj.TileLocation = pos;
                        loc.objects[pos] = obj;
                        tree.tapped.Value = true;
                        tree.UpdateTapperProduct(obj);
                        return;
                    }
                    obj = new(pos, id);
                    break;
                case 248:
                    obj = new Chest(playerChest: true, pos, id)
                    {
                        SpecialChestType = Chest.SpecialChestTypes.MiniShippingBin
                    }; break;
                case 256:
                    obj = new Chest(playerChest: true, pos, id)
                    {
                        SpecialChestType = Chest.SpecialChestTypes.JunimoChest
                    }; break;
                case 275:
                    obj = new Chest(true, pos, id)
                    {
                        SpecialChestType = Chest.SpecialChestTypes.AutoLoader
                    };
                    (obj as Chest).lidFrameCount.Value = 2;
                    break;
                case >= 143 and <= 153: //torch fix
                    obj = new Torch(pos, id, true);
                    break;
                default:
                    obj = new(pos, id);
                    break;
            }
            loc.objects[pos] = obj;
            
        }
        public static void AddFruitTree(GameLocation loc, Vector2 pos, string str)
        {
            if (!int.TryParse(str, out int id))
                return;

            FruitTree tree = new(id, 4);
            tree.daysUntilMature.Value = 0;
            loc.terrainFeatures[pos] = tree;
        }
    }
}
