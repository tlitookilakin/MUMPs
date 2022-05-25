using AeroCore;
using AeroCore.Models;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Integration
{
    [ModInit(WhenHasMod = "shekurika.WaterFish")]
    class VisibleFish
    {
        private static (double chance, string loc)? farmFishOverride = null;

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
            var method = AccessTools.TypeByName("showFishInWater.FishManager").MethodNamed("getFishAt");
            ModEntry.harmony.Patch(method, transpiler: new HarmonyMethod(typeof(VisibleFish), "Transpiler"));
            ModEntry.helper.Events.GameLoop.DayStarted += (s, e) => DayStart();
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
