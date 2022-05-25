using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MUMPs.Patches
{
    class BlockedTileClearer
    {
        public static void ClearBlockedTilesIn(GameLocation loc)
        {
            //don't run in farmhouse, it interacts weirdly with house upgrades
            if (loc is null || loc.map == null || loc is FarmHouse || !loc.map.Properties.ContainsKey("ClearBlockedTiles"))
                return;

            List<Vector2> objPositions = loc.Objects.Keys.ToList();
            foreach (Vector2 pos in objPositions)
            {
                // allow water placement to prevent chest dumping and crab pot deletion
                if(ShouldKillObject(loc, pos, true))
                {
                    ClearObject(loc.Objects[pos], loc, pos);
                    loc.Objects.Remove(pos);
                }
            }
            loc.furniture.Filter((f) => {
                if (ShouldKillObject(loc, f.TileLocation))
                {
                    ClearFurniture(f, loc);
                    return false;
                }
                return true;
            });
            //just delete these, no drops.
            loc.resourceClumps.Filter((f) =>
            {
                return !ShouldKillObject(loc, f.tile.Value);
            });
            loc.terrainFeatures.Filter((f) => {
                return !ShouldKillObject(loc, f.Key);
            });
            loc.largeTerrainFeatures.Filter((f) => {
                return !ShouldKillObject(loc, f.tilePosition.Value);
            });
        }
        public static void ClearFurniture(Furniture obj, GameLocation loc)
        {
            Vector2 pos = obj.TileLocation;
            Vector2 pixelPos = new(pos.X * 64f + 32f, pos.Y * 64f + 32f);
            Game1.createItemDebris(obj.getOne(), pixelPos, 0, loc);
            if(obj is StorageFurniture furn)
            {
                foreach(Item item in furn.heldItems)
                {
                    Game1.createItemDebris(item, pixelPos, Game1.random.Next(4), loc);
                }
            }
            obj.performRemoveAction(pos, loc);
        }
        public static void ClearObject(StardewValley.Object obj, GameLocation loc, Vector2 pos)
        {
            Vector2 pixelPos = new(pos.X * 64f + 32f, pos.Y * 64f + 32f);
            if (obj is Chest chest)
            {
                if (chest.SpecialChestType is (Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin))
                    return;
                if (!chest.MoveToSafePosition(loc, pos))
                {
                    Game1.createItemDebris(obj.getOne(), pixelPos, 0, loc);
                    chest.destroyAndDropContents(pixelPos, loc);
                }
            }
            obj.performRemoveAction(pos, loc);
        }
        public static bool ShouldKillObject(GameLocation loc, Vector2 tilePosition, bool allowWater = false)
        {
            return ShouldKillObject(loc, (int)tilePosition.X, (int)tilePosition.Y, allowWater);
        }
        public static bool ShouldKillObject(GameLocation loc, int x, int y, bool allowWater = false)
        {
            return  !(allowWater && loc.isWaterTile(x, y)) && (
                    !loc.isTilePassable(new xTile.Dimensions.Location(x, y), Game1.viewport) ||
                    loc.doesTileHaveProperty(x, y, "Placeable", "Back") != null);
        }
    }
}
