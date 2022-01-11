using System;
using MUMPs.models;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI.Utilities;

namespace MUMPs.Props
{
    [HarmonyPatch]
    class Horizon
    {
        private static readonly Dictionary<string, HorizonModel> Templates = new(StringComparer.OrdinalIgnoreCase);
        private static readonly PerScreen<IDrawableWorldLayer> currentHorizon = new();
        private static readonly PerScreen<IDrawableWorldLayer> currentForeground = new();
        public static IDrawableWorldLayer getTemplate(string name)
        {
            if (name.Length == 0)
                return null;
            if(name.ToLower() == "summit")
            {
                return new SummitHorizon();
            }
            if(!Templates.TryGetValue(name, out HorizonModel ret))
            {
                try
                {
                    ret = ModEntry.helper.Content.Load<HorizonModel>("Data/Mumps/Backgrounds/" + name);
                }
                catch (ContentLoadException e)
                {
                    ModEntry.monitor.Log("Could not find background template '" + name + "'.\nReason: "+e.Message, LogLevel.Warn);
                    return null;
                }
                Templates.Add(name, ret);
            }
            return ret;
        }
        public static void ChangeLocation(GameLocation loc)
        {
            currentHorizon.Value = null;
            currentForeground.Value = null;
            if(loc == null)
                return;
            currentHorizon.Value = getTemplate(loc.getMapProperty("Horizon").Trim());
            currentForeground.Value = getTemplate(loc.getMapProperty("Foreground").Trim());
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
            currentHorizon.Value = null;
            currentForeground.Value = null;
        }
    }
}
