using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUMPs.Patches
{
    [HarmonyPatch(typeof(GameLocation))]
    class Action
    {
        public static readonly Dictionary<string, Action<Farmer, string, Point>> actions = new();
        public static readonly List<string> inspectActions = new();

        [HarmonyPatch("performAction")]
        [HarmonyPrefix]
        public static bool performAction(string action, Farmer who, ref bool __result, xTile.Dimensions.Location tileLocation)
        {
            if (action == null || !who.IsLocalPlayer)
                return true;

            string[] vals = action.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length < 1)
                return true;

            string name = vals[0];
            if (!actions.TryGetValue(name, out var exec))
                return true;

            StringBuilder sb = new();
            for(int i = 1; i < vals.Length; i++)
            {
                sb.Append(vals[i]);
                if (i + 1 < vals.Length)
                    sb.Append(' ');
            }

            __result = true;
            exec(who,sb.ToString(), new(tileLocation.X, tileLocation.Y));
            return false;
        }

        [HarmonyPatch("isActionableTile")]
        [HarmonyPostfix]
        public static void checkCursor(int xTile, int yTile, GameLocation __instance, ref bool __result)
        {
            string action = __instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings");
            if (action == null)
                return;

            string[] vals = action.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length < 1)
                return;

            string name = vals[0];
            __result = __result || actions.ContainsKey(name);
            Game1.isInspectionAtCurrentCursorTile = Game1.isInspectionAtCurrentCursorTile || inspectActions.Contains(name);
        }
    }
}
