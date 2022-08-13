using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
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
            HarmonyMethod furnitureSkip = new(typeof(NoInteract).MethodNamed(nameof(CancelFurniture)));
            HarmonyMethod objectPatch = new(typeof(NoInteract).MethodNamed(nameof(NoInteractObject)));
            HarmonyMethod objectPickup = new(typeof(NoInteract).MethodNamed(nameof(NoPickupObject)));
            HarmonyMethod cancelCraftable = new(typeof(NoInteract).MethodNamed(nameof(CraftableCancelProduce)));

            foreach (var type in Reflection.GetAllKnownTypes()) 
            {
                if (type.IsAssignableTo(typeof(TerrainFeature))) 
                {
                    ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"performToolAction"), prefix: terrainSkip);
                } 
                else
                {
                    if (type.IsAssignableTo(typeof(Furniture)))
                    {
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"rotate"), furnitureSkip);
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"hoverAction"), furnitureSkip);
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type, "canBeRemoved"), objectPickup);
                    } 
                    if (type.IsAssignableTo(typeof(SObject)))
                    {
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"performToolAction"), prefix: objectPickup);
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"performObjectDropInAction"), prefix: objectPatch);
                        ModEntry.harmony.TryPatch(AccessTools.DeclaredMethod(type,"checkForAction"), prefix: objectPatch);
                    }
                }
            }

            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.performUseAction)), terrainSkip);
            ModEntry.harmony.Patch(typeof(HoeDirt).MethodNamed(nameof(HoeDirt.readyForHarvest)), terrainSkip);
            ModEntry.harmony.Patch(typeof(FishTankFurniture).MethodNamed(nameof(FishTankFurniture.CanBeDeposited)), objectPatch);
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.minutesElapsed)), objectPatch);
            ModEntry.harmony.Patch(typeof(Fence).MethodNamed(nameof(Fence.minutesElapsed)), objectPickup);
            ModEntry.harmony.Patch(typeof(Crop).MethodNamed(nameof(Crop.newDay)),
                postfix: new(typeof(NoInteract).MethodNamed(nameof(PropogateToGiant))));
            ModEntry.harmony.Patch(typeof(SObject).MethodNamed(nameof(SObject.DayUpdate)),
                postfix: cancelCraftable);
        }
        private static bool NoInteractObject(SObject __instance, ref bool __result)
        {
            bool noInteract = __instance.modData.ContainsKey("tlitookilakin.mumps.noInteract");
            __result = !noInteract && __result;
            return !noInteract;
        }
        private static bool NoPickupObject(SObject __instance, ref bool __result) 
        {
            bool noPickup = __instance.modData.ContainsKey("tlitookilakin.mumps.noPickup");
            __result = !noPickup && __result;
            return !noPickup;
        }
        private static void CraftableCancelProduce(SObject __instance)
        {
            if (__instance.modData.ContainsKey("tlitookilakin.mumps.noInteract"))
                __instance.readyForHarvest.Value = false;
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
            if (environment.terrainFeatures.TryGetValue(new(xTile, yTile), out var tf))
            {
                var clump = environment.ResourceClumpAt(new(xTile - 1f, yTile - 1f));
                if (clump is GiantCrop and not null)
                {
                    if (tf.modData.ContainsKey("tlitookilakin.mumps.noInteract"))
                        clump.modData["tlitookilakin.mumps.noInteract"] = "T";
                }
            }
        }
    }
}
