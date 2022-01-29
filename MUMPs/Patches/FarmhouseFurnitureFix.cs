using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Patches
{
    [HarmonyPatch]
    class FarmhouseFurnitureFix
    {
        private static Dictionary<int, string> furnitureData;

        [HarmonyPatch(typeof(FarmHouse), MethodType.Constructor, new Type[]{typeof(string), typeof(string)})]
        [HarmonyPostfix]
        public static void FixFurniture(FarmHouse __instance)
        {
            furnitureData = ModEntry.helper.Content.Load<Dictionary<int, string>>("Data/Furniture", ContentSource.GameContent);
            __instance.furniture.TransformItems(Replace);
        }
        public static Furniture Replace(Furniture orig)
        {
            if (Props.SpawnObject.TVIDs.Contains(orig.parentSheetIndex))
                return (orig is TV) ? orig : new TV(orig.parentSheetIndex, orig.tileLocation);
            else if (furnitureData.TryGetValue(orig.parentSheetIndex, out string data))
                switch (data.Split('/')[1].Split(' ')[0])
                {
                    case "bed":
                        return (orig is BedFurniture) ? orig : new BedFurniture(orig.parentSheetIndex, orig.tileLocation, orig.currentRotation);
                    case "fishtank":
                        return (orig is FishTankFurniture) ? orig : new FishTankFurniture(orig.parentSheetIndex, orig.tileLocation, orig.currentRotation);
                }
            return orig;
        }
    }
}
