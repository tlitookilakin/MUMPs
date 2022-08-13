using AeroCore;
using AeroCore.Models;
using AeroCore.ReflectedValue;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace MUMPs.Integration
{
    [ModInit(WhenHasMod = "shekurika.WaterFish")]
    class VisibleFish
    {
        private static (double chance, string loc)? farmFishOverride = null;
        private static ValueChain addTrash;

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator gen) => patcher.Run(codes, gen);
        private static readonly ILHelper patcher = new ILHelper(ModEntry.monitor, "Visible Fish Integration")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldloc_0),
                new(OpCodes.Callvirt, typeof(GameLocation).MethodNamed("get_NameOrUniqueName")),
                new(OpCodes.Stloc_S, (-1, typeof(string)))
            })
            .Skip(2)
            .Transform(Inject)
            .Remove(1)
            .Finish();

        internal static void Init()
        {
            ModEntry.monitor.Log("Attempting to patch Visible Fish mod for integration...", LogLevel.Debug);
            ModEntry.helper.Events.GameLoop.DayStarted += (s, e) => DayStart();

            var manager = AccessTools.TypeByName("showFishInWater.FishManager");
            if (manager is null)
            {
                ModEntry.monitor.Log("Could not find FishManager; failed to patch", LogLevel.Debug);
                return;
            }
            if (!ModEntry.harmony.TryPatch(manager.MethodNamed("getFishAt"), transpiler: new(typeof(VisibleFish), nameof(Transpiler))))
                ModEntry.monitor.Log("Could not find target method 'getFishAt'; failed to patch.", LogLevel.Debug);
            if (!ModEntry.harmony.TryPatch(manager.MethodNamed("SpawnSpecialObjects"), postfix: new(typeof(VisibleFish), nameof(SpawnObjects))))
                ModEntry.monitor.Log("Could not find target method 'SpawnSpecialObjects'; failed to patch.", LogLevel.Debug);
            addTrash = manager.ValueRef("fishTank")?.MethodRef<object>("addTrash", typeof(SObject), typeof(int), typeof(int), typeof(bool));
        }

        private static void SpawnObjects(GameLocation location, object __instance)
        {
            if (location?.Map is null || addTrash is null)
                return;

            foreach ((var tile, int x, int y) in location.Map.TilesInLayer("Back"))
            {
                if (!tile.TileHasProperty("FishingTreasure", out var prop))
                    continue;
                var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 1 || !split[0].TryGetItem(out Item fished) || fished is not SObject fobj)
                    continue;
                if (split.Length < 2 || !(split.Length < 3 ? Game1.MasterPlayer : Game1.player).hasOrWillReceiveMail(split[1]))
                    addTrash.Call(__instance, null, fobj, x, y, false);
            }
        }

        private static void DayStart()
        {
            string[] prop = Game1.getFarm().getMapProperty("FarmFishLocationOverride")?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (prop != null && prop.Length >= 2 && double.TryParse(prop[1], out double chance))
                farmFishOverride = (chance, prop[0]);
            else
                farmFishOverride = null;
        }

        private static string GetUseNameAt(int x, int y, string defaultVal)
        {
            Point pos = new(x, y);

            foreach((Rectangle rect, string use) in Props.FishingArea.locRegions.Value)
                if (rect.Contains(pos))
                    return use;

            if (Game1.currentLocation is Farm && farmFishOverride.HasValue && Game1.random.NextDouble() <= farmFishOverride.Value.chance)
                return farmFishOverride.Value.loc;

            return defaultVal;
        }

        private static IList<CodeInstruction> Inject(ILHelper.ILEnumerator cursor)
        {
            return new CodeInstruction[]
            {
                cursor.Current,
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldloc_S, cursor.Current.operand),
                new(OpCodes.Call, typeof(VisibleFish).MethodNamed(nameof(GetUseNameAt))),
                new(OpCodes.Stloc_S, cursor.Current.operand)
            };
        }
    }
}
