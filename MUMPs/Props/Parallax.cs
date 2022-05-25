using System;
using MUMPs.models;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework;
using AeroCore.Utils;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class Parallax
    {
        private static readonly PerScreen<HorizonModel> currentBackground = new();
        private static readonly PerScreen<HorizonModel> currentForeground = new();
        private static readonly PerScreen<Vector2> backgroundOffset = new();
        private static readonly PerScreen<Vector2> foregroundOffset = new();
        public static HorizonModel getTemplate(string prop, out Vector2 offset)
        {
            string[] props = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            offset = Vector2.Zero;

            if (props.Length == 0)
                return null;

            if (props.ToVec2(out Vector2 vec, 1))
                offset = vec;
            else
                offset = Vector2.Zero;

            if (Misc.TryLoadAsset<HorizonModel>(ModEntry.monitor, ModEntry.helper, "Mods/Mumps/Backgrounds/" + props[0], out var ret))
                return ret;
            else
                return null;
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentBackground.Value.Dispose();
            currentForeground.Value.Dispose();
            currentBackground.Value = null;
            currentForeground.Value = null;

            if(loc is null)
                return;

            currentBackground.Value = getTemplate(loc.getMapProperty("Background"), out Vector2 off);
            backgroundOffset.Value = off;
            currentForeground.Value = getTemplate(loc.getMapProperty("Foreground"), out off);
            foregroundOffset.Value = off;
        }

        [HarmonyPatch(typeof(GameLocation), "drawBackground")]
        [HarmonyPrefix]
        public static void DrawBackgroundPrefix(ref SpriteBatch b) => currentBackground.Value?.Draw(b, false, backgroundOffset.Value);
        public static void DrawAfter(SpriteBatch b) => currentForeground.Value?.Draw(b, true, foregroundOffset.Value);
        public static void Cleanup()
        {
            currentBackground.Value.Dispose();
            currentForeground.Value.Dispose();
            currentBackground.ResetAllScreens();
            currentForeground.ResetAllScreens();
            backgroundOffset.ResetAllScreens();
            foregroundOffset.ResetAllScreens();
        }
    }
}
