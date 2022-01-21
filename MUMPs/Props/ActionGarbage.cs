using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class ActionGarbage
    {
        const int trashIndex = 78;
        private static ILHelper patcher = setupHelper();
        public static readonly PerScreen<Dictionary<GameLocation, HashSet<string>>> checkedCans = new(() => new());

        public static void HandleAction(Farmer who, string action)
        {
            if(who.currentLocation == null)
                return;

            string id = action.Trim();

            if (!checkedCans.Value.TryGetValue(who.currentLocation, out var cans))
            {
                cans = new();
                checkedCans.Value.Add(who.currentLocation,cans);
            }
            if (!cans.Contains(id))
            {
                cans.Add(id);
                Point mouse = Utils.LocalToGlobal(Game1.getMousePositionRaw());
                DoGarbage(who.currentLocation, new(mouse.X / 64, mouse.Y / 64), Game1.viewport, who);
            }
        }

        private static ILHelper setupHelper()
        {
            ILHelper helper = new("Garbage");
            helper.RemoveAt(new CodeInstruction[]{
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(Town).FieldNamed("garbageChecked")),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldc_I4_1)
            }).Remove().SkipTo(new CodeInstruction[]{
                new(OpCodes.Add),
                new(OpCodes.Ldc_I4, 777),
                new(OpCodes.Add)
            }).Remove().Add(
                new CodeInstruction(OpCodes.Ldc_I4_4)
            ).SkipTo(new CodeInstruction[]{
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Stsfld, typeof(Game1).FieldNamed("haltAfterCheck")),
                new(OpCodes.Ldc_I4_1)
            }).Stop();
            return helper;
        }

        [HarmonyPatch(typeof(Town),"checkAction")]
        [HarmonyReversePatch]
        public static void DoGarbage(GameLocation instance, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {/* broken for now, fix later
            //reverse patch transpiler
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var code in patcher.Run(instructions))
                    yield return code;
            }

            // make the compiler happy
            _ = Transpiler(null);*/
        }

        [HarmonyPatch(typeof(Town),"checkAction")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static GameLocation replaceLocation(GameLocation loc)
        {
            return Game1.currentLocation;
        }
        public static GameLocation spoofedLocation(GameLocation loc, object list)
        {
            return Game1.currentLocation;
        }
    }
}
