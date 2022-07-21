using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
    [ModInit]
    [HarmonyPatch]
    internal class CrabPotArea
    {
        private static Dictionary<GameLocation, Dictionary<Rectangle, List<string>>> regions = new();
        private static Dictionary<GameLocation, string[]> defaultRegion = new();

        internal static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayStarted += (s, e) => OnDayStart();
        }
        private static void OnDayStart()
        {
            regions.Clear();
            defaultRegion.Clear();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CrabPot), nameof(CrabPot.DayUpdate))]
        internal static IEnumerable<CodeInstruction> RunPatch(ILGenerator Generator, IEnumerable<CodeInstruction> instructions)
            => CrabPotDayUpdatePatcher.Run(instructions, Generator);

        private static ILHelper CrabPotDayUpdatePatcher = new ILHelper(ModEntry.monitor, "crab pot day update")
            .SkipTo(new CodeInstruction(OpCodes.Ldarg_0))
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(CrabPotArea).MethodNamed(nameof(GetPotZone)))
            })
            .StoreLocal("zone", typeof(IList<string>))
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldc_I4_S, 47),
                new(OpCodes.Ldc_I4_0)
            })
            .Skip(4)
            .LoadLocal("zone", typeof(IList<string>))
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Ldc_I4_4),
                new(OpCodes.Ldelem_Ref),
                new(OpCodes.Callvirt, typeof(ICollection<string>).MethodNamed(nameof(ICollection<string>.Contains)))
            })
            .AddJump(OpCodes.Brtrue_S, "matched")
            .AddJump(OpCodes.Br_S, "continue")
            .SkipTo(new CodeInstruction(OpCodes.Ldloc_1))
            .AddLabel("matched")
            .SkipTo(new CodeInstruction(OpCodes.Leave_S))
            .Skip(1)
            .AddLabel("continue")
            .Finish();

        private static IList<string> GetPotZone(GameLocation loc, CrabPot pot)
        {
            if (!defaultRegion.ContainsKey(loc))
                FindRegions(loc);

            var pt = pot.TileLocation.ToPoint();
            var locs = GetZone(loc, pt);
            if (!loc.catchOceanCrabPotFishFromThisSpot(pt.X, pt.Y) || locs.Contains("ocean"))
                return locs;
            return new List<string>(locs){"ocean"};
        }
        private static IList<string> GetZone(GameLocation loc, Point tile)
        {
            foreach ((var region, var id) in regions[loc])
                if (region.Contains(tile))
                    return id;
            return defaultRegion[loc];
        }
        private static void FindRegions(GameLocation where)
        {
            defaultRegion[where] = 
                where.getMapProperty("DefaultCrabPotArea")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) 
                ?? new[] { where is Beach ? "ocean" : "freshwater" };

            Dictionary<Rectangle, List<string>> reg = new();
            var split = Maps.MapPropertyArray(where, "CrabPotAreaCorners");
            for(int i = 0; i + 4 < split.Length;)
            {
                if (!split.FromCorners(out var region, i))
                {
                    ModEntry.monitor.Log($"CrabPotAreaCorners map property is not valid in location '{where.Name}'", LogLevel.Warn);
                    break;
                }
                i += 4;
                List<string> ids = new();
                while (!int.TryParse(split[i], out var _) && i < split.Length)
                {
                    ids.Add(split[i]);
                    i++;
                }
                reg[region] = ids;
            }
            regions[where] = reg;
        }
    }
}
