using System;
using MUMPs.models;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
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
        private static readonly Dictionary<string, HorizonModel> Templates = new(StringComparer.OrdinalIgnoreCase);
        private static readonly PerScreen<IDrawableWorldLayer> currentBackground = new();
        private static readonly PerScreen<IDrawableWorldLayer> currentForeground = new();
        private static readonly PerScreen<Vector2> backgroundOffset = new();
        private static readonly PerScreen<Vector2> foregroundOffset = new();
        public static IDrawableWorldLayer getTemplate(string prop, out Vector2 offset)
        {
            string[] props = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            offset = Vector2.Zero;

            if (props.Length == 0)
                return null;

            if (props.ToVec2(out Vector2 vec))
                offset = vec;
            else
                offset = Vector2.Zero;

            if (props[0].ToLowerInvariant() == "summit")
            {
                return new SummitHorizon();
            }
            if(!Templates.TryGetValue(props[0], out HorizonModel ret))
            {
                try
                {
                    ret = ModEntry.helper.Content.Load<HorizonModel>("Mods/Mumps/Backgrounds/" + props[0], ContentSource.GameContent);
                }
                catch (ContentLoadException e)
                {
                    ModEntry.monitor.Log("Could not find background template '" + props[0] + "'.\nReason: "+e.Message, LogLevel.Warn);
                    return null;
                }
                Templates.Add(props[0], ret);
            }
            return ret;
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentBackground.Value = null;
            currentForeground.Value = null;

            if(loc == null)
                return;

            Vector2 off;
            currentBackground.Value = getTemplate(loc.getMapProperty("Background"), out off);
            backgroundOffset.Value = off;
            currentForeground.Value = getTemplate(loc.getMapProperty("Foreground"), out off);
            foregroundOffset.Value = off;
        }

        [HarmonyPatch(typeof(GameLocation), "drawBackground")]
        [HarmonyPrefix]
        public static void DrawBackgroundPrefix(ref SpriteBatch b)
        {
            currentBackground.Value?.Draw(b, false, backgroundOffset.Value);
        }
        public static void DrawAfter(SpriteBatch b)
        {
            currentForeground.Value?.Draw(b, true, foregroundOffset.Value);
        }
        public static void Cleanup()
        {
            currentBackground.ResetAllScreens();
            currentForeground.ResetAllScreens();
            backgroundOffset.ResetAllScreens();
            foregroundOffset.ResetAllScreens();
        }
    }
}
