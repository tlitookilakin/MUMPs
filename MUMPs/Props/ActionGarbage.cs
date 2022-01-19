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
        private static bool spoofing = false;
        private static ILHelper patcher = setupHelper();

        private static ILHelper setupHelper()
        {
            ILHelper helper = new("Garbage");
            helper.SkipTo(new CodeInstruction[]{
                new(OpCodes.Ldsfld, typeof(Game1).FieldNamed("multiplayer")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_S, (-1, typeof(List<TemporaryAnimatedSprite>)))
            }).Add(new CodeInstruction[] {
                new(OpCodes.Call, typeof(ActionGarbage).MethodNamed("spoofedLocation")),
                new(OpCodes.Ldloc_S, 9)
            }).SkipTo(new CodeInstruction[]{
                new(OpCodes.Ldc_I4_7),
                new(OpCodes.Ldarg_0)
            }).Add(new CodeInstruction[] {
                new(OpCodes.Call, typeof(ActionGarbage).MethodNamed("replaceLocation"))
            }).Finish();
            return helper;
        }

        [HarmonyPatch(typeof(GameLocation),"checkAction")]
        [HarmonyPrefix]
        public static void checkGarbage(xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, GameLocation __instance)
        {
            if (__instance is Town)
                return;

            if((__instance.map.GetLayer("Buildings").Tiles[tileLocation]?.TileIndex ?? 0) == trashIndex)
            {
                spoofing = true;
                (Game1.getLocationFromName("Town") as Town).checkAction(tileLocation, viewport, who);
            }
        }

        [HarmonyPatch(typeof(Town),"checkAction")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static int spoofedIndex(int original)
        {
            if (spoofing)
            {
                spoofing = false;
                return trashIndex;
            }
            return original;
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
