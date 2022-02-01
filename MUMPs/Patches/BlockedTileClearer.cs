using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Patches
{
    class BlockedTileClearer
    {
        public static void ClearBlockedTilesIn(GameLocation loc)
        {
            if (loc?.map == null)
                return;

            List<Vector2> objPositions = loc.Objects.Keys.ToList();
            foreach (Vector2 pos in objPositions)
            {
                if(ShouldKillObject(loc, (int)pos.X, (int)pos.Y))
                {
                    ClearObject(loc.Objects[pos], loc, pos);
                    loc.Objects.Remove(pos);
                }
            }
            loc.resourceClumps.Filter((f) =>
            {
                return !ShouldKillObject(loc, f.tile);
            });
            loc.terrainFeatures.Filter((f) => {
                return !ShouldKillObject(loc, f.Key);
            });
            loc.largeTerrainFeatures.Filter((f) => {
                return !ShouldKillObject(loc, f.tilePosition);
            });
            loc.furniture.Filter((f) => {
                if(ShouldKillObject(loc, f.TileLocation))
                {
                    ClearFurniture(f, loc);
                    return false;
                }
                return true;
            });
        }
        public static void ClearFurniture(Furniture obj, GameLocation loc)
        {
            Vector2 pos = obj.tileLocation;
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
            if (obj is Chest chest)
            {
                if (chest.SpecialChestType is (Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin))
                    return;
                if (!chest.MoveToSafePosition(loc, pos))
                {
                    Game1.createItemDebris(obj.getOne(), new(pos.X * 64f + 32f, pos.Y * 64f + 32f), 0, loc);
                    chest.destroyAndDropContents(pos, loc);
                }
            }
            obj.performRemoveAction(pos, loc);
        }
        public static bool ShouldKillObject(GameLocation loc, Vector2 tilePosition)
        {
            return ShouldKillObject(loc, (int)tilePosition.X, (int)tilePosition.Y);
        }
        public static bool ShouldKillObject(GameLocation loc, int x, int y)
        {
            return loc.isWaterTile(x, y) ||
                    !loc.isTilePassable(new xTile.Dimensions.Location(x, y), Game1.viewport) ||
                    loc.doesTileHaveProperty(x, y, "Placeable", "Back") != null;
        }
    }
}
