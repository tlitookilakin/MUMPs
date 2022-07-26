using AeroCore.Utils;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MUMPs.Patches
{
    [HarmonyPatch]
    class FarmhouseFurnitureFix
    {
        private static Dictionary<int, string> furnitureData; 
        internal static int[] TVIDs = {1466, 1468, 1680, 2326};

        [HarmonyPatch(typeof(FarmHouse), MethodType.Constructor, new Type[]{typeof(string), typeof(string)})]
        [HarmonyPostfix]
        public static void FixFurniture(FarmHouse __instance)
        {
            if (__instance.modData.ContainsKey("tlitoo.mumps.fixedFurniture"))
                return;

            furnitureData = ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Furniture");
            __instance.furniture.TransformItems(Replace);
            __instance.modData.Add("tlitoo.mumps.fixedFurniture", "T");
        }
        public static Furniture Replace(Furniture orig)
        {
            if (TVIDs.Contains(orig.ParentSheetIndex))
                return (orig is TV) ? orig : new TV(orig.ParentSheetIndex, orig.TileLocation);
            else if (furnitureData.TryGetValue(orig.ParentSheetIndex, out string data))
                switch (data.GetChunk('/', 1).GetChunk(' ', 0)) {
                    case "fishtank":
                        return (orig is FishTankFurniture) ? orig : new FishTankFurniture(orig.ParentSheetIndex, orig.TileLocation, orig.currentRotation.Value);
                    case "bed":
                        return (orig is BedFurniture) ? orig : new BedFurniture(orig.ParentSheetIndex, orig.TileLocation);
                }
            return orig;
        }
    }
}
