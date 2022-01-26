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

namespace MUMPs.Props
{
    [HarmonyPatch]
    class Horizon
    {
        private static readonly Dictionary<string, HorizonModel> Templates = new(StringComparer.OrdinalIgnoreCase);
        private static readonly PerScreen<IDrawableWorldLayer> currentHorizon = new();
        private static readonly PerScreen<IDrawableWorldLayer> currentForeground = new();
        public static IDrawableWorldLayer getTemplate(string prop)
        {
            string[] props = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (props.Length == 0)
                return null;

            if(props[0].ToLowerInvariant() == "summit")
            {
                return new SummitHorizon();
            }
            if(!Templates.TryGetValue(props[0], out HorizonModel ret))
            {
                try
                {
                    ret = ModEntry.helper.Content.Load<HorizonModel>("Mods/Mumps/Backgrounds/" + props[0]);
                }
                catch (ContentLoadException e)
                {
                    ModEntry.monitor.Log("Could not find background template '" + props[0] + "'.\nReason: "+e.Message, LogLevel.Warn);
                    return null;
                }
                Templates.Add(props[0], ret);
            }
            if (props.StringsToVec2(out Vector2 vec))
                ret.offset = vec;
            else
                ret.offset = Vector2.Zero;
            return ret;
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentHorizon.Value = null;
            currentForeground.Value = null;
            if(loc == null)
                return;
            currentHorizon.Value = getTemplate(loc.getMapProperty("Background"));
            currentForeground.Value = getTemplate(loc.getMapProperty("Foreground"));
        }
        [HarmonyPatch(typeof(GameLocation), "drawBackground")]
        [HarmonyPrefix]
        public static void DrawBackgroundPrefix(ref SpriteBatch b)
        {
            currentHorizon.Value?.Draw(b, false);
        }
        public static void DrawAfter(SpriteBatch b)
        {
            currentForeground.Value?.Draw(b, true);
        }
        public static void Cleanup()
        {
            currentHorizon.ResetAllScreens();
            currentForeground.ResetAllScreens();
        }
    }
}
