using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MUMPs.Patches
{
    [ModInit]
    internal class NoInteract
    {
        internal static void Init()
        {
            HarmonyMethod terrainSkip = new(typeof(NoInteract).MethodNamed(nameof(NoInteractTerrainFeature)));
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.performToolAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.performUseAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.readyForHarvest)), terrainSkip);
            ModEntry.harmony.Patch(typeof(Crop).MethodNamed(nameof(Crop.newDay)), 
                postfix: new(typeof(NoInteract).MethodNamed(nameof(PropogateToGiant))));
            ModEntry.harmony.Patch(typeof(FruitTree).MethodNamed(nameof(FruitTree.performToolAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(GiantCrop).MethodNamed(nameof(GiantCrop.performToolAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(Flooring).MethodNamed(nameof(Flooring.performToolAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(CosmeticPlant).MethodNamed(nameof(CosmeticPlant.performToolAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(Tree).MethodNamed(nameof(Tree.performToolAction)), terrainSkip);

            HarmonyMethod objectPatch = new(typeof(NoInteract).MethodNamed(nameof(NoInteractObject)));
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.performObjectDropInAction)), objectPatch);
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.performToolAction)), objectPatch);
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.checkForAction)), objectPatch);
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.minutesElapsed)), objectPatch);
            ModEntry.harmony.Patch(typeof(Fence).MethodNamed(nameof(Fence.performToolAction)), objectPatch);
            ModEntry.harmony.Patch(typeof(Fence).MethodNamed(nameof(Fence.minutesElapsed)), objectPatch);
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.DayUpdate)), 
                postfix: new(typeof(NoInteract).MethodNamed(nameof(CraftableCancelProduce))));

            HarmonyMethod furnitureSkip = new(typeof(NoInteract).MethodNamed(nameof(CancelFurniture)));
            ModEntry.harmony.Patch(typeof(Furniture).MethodNamed(nameof(Furniture.canBeRemoved)), objectPatch);
            ModEntry.harmony.Patch(typeof(Furniture).MethodNamed(nameof(Furniture.rotate)), furnitureSkip);
            ModEntry.harmony.Patch(typeof(Furniture).MethodNamed(nameof(Furniture.hoverAction)), furnitureSkip);
        }
        private static bool NoInteractObject(SObject __instance, ref bool __result)
        {
            bool noInteract = __instance.modData.ContainsKey("tlitookilakin.mumps.noInteract");
            __result = !noInteract && __result;
            return !noInteract;
        }
        private static void CraftableCancelProduce(SObject __instance)
        {
            if (__instance.modData.ContainsKey("tlitookilakin.mumps.noInteract"))
            {
                __instance.readyForHarvest.Value = false;
                __instance.heldObject.Value = null;
            }
        }
        private static bool CancelFurniture(Furniture __instance)
            => !__instance.modData.ContainsKey("tlitookilakin.mumps.noInteract");
        private static bool NoInteractTerrainFeature(TerrainFeature __instance, ref bool __result)
        {
            bool noInteract = __instance.modData.ContainsKey("tlitookilakin.mumps.noInteract");
            __result = !noInteract && __result;
            return !noInteract;
        }
        private static void PropogateToGiant(int xTile, int yTile, GameLocation environment)
        {
            if (environment.terrainFeatures.TryGetValue(new(xTile, yTile), out var tf) &&
                tf.modData.ContainsKey("tlitookilakin.mumps.noInteract"))
            {
                var clump = environment.ResourceClumpAt(new(xTile - 1f, yTile - 1f));
                if (clump is null)
                    return;
                if (clump is GiantCrop)
                    clump.modData["tlitookilakin.mumps.noInteract"] = "T";
            }
        }
    }
}
