using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.Integration
{
    class VisibleFish
    {
        private static readonly ILHelper patcher = setupPatcher();
        private static (double chance, string loc)? farmFishOverride = null;
        public static void Setup()
        {
            if (ModEntry.helper.ModRegistry.IsLoaded("shekurika.WaterFish"))
            {
                ModEntry.monitor.Log("Attempting to patch Visible Fish mod for integration...", LogLevel.Debug);
                var method = Utils.TypeNamed("showFishInWater.FishManager").MethodNamed("getFishAt");
                ModEntry.harmony.Patch(method, transpiler: new HarmonyMethod(typeof(VisibleFish), "Transpiler"));
            }
        }
        public static void DayStart()
        {
            string[] prop = Game1.getFarm().getMapProperty("FarmFishLocationOverride")?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (prop != null && prop.Length >= 2 && double.TryParse(prop[1], out double chance))
                farmFishOverride = (chance, prop[0]);
            else
                farmFishOverride = null;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static string GetUseNameAt(GameLocation loc, int x, int y, string defaultVal)
        {
            Point pos = new(x, y);

            foreach((Rectangle rect, string use) in Props.FishingArea.locRegions.Value)
                if (rect.Contains(pos))
                    return use;

            if (Game1.currentLocation is Farm && farmFishOverride.HasValue && Game1.random.NextDouble() <= farmFishOverride.Value.chance)
                return farmFishOverride.Value.loc;

            return defaultVal;
        }
        private static ILHelper setupPatcher()
        {
            return new ILHelper("Visible Fish").SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldloc_0),
                new(OpCodes.Callvirt, typeof(GameLocation).MethodNamed("get_NameOrUniqueName")),
                new(OpCodes.Stloc_S, (-1, typeof(string)))
            }).Add(Inject).Finish();
        }
        private static IEnumerable<CodeInstruction> Inject(IList<LocalBuilder> boxes)
        {
            yield return new(OpCodes.Ldarg_0);
            yield return new(OpCodes.Ldarg_1);
            yield return new(OpCodes.Ldarg_2);
            yield return new(OpCodes.Ldloc_S, boxes[0]);
            yield return new(OpCodes.Call, typeof(VisibleFish).MethodNamed("GetUseNameAt"));
            yield return new(OpCodes.Stloc_S, boxes[0]);
        }
    }
}
