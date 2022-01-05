using System;
using MUMPs.models;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;

namespace MUMPs.Props
{
    class Horizon
    {
        private static Dictionary<string, HorizonModel> Templates = new(StringComparer.OrdinalIgnoreCase);
        private static IDrawableWorldLayer currentHorizon = null;
        private static IDrawableWorldLayer currentForeground = null;
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
            currentHorizon = null;
            currentForeground = null;
            if(loc == null)
                return;
            currentHorizon = getTemplate(loc.getMapProperty("Horizon").Trim());
            currentForeground = getTemplate(loc.getMapProperty("Foreground").Trim());
        }
        public static void DrawBackgroundPrefix(ref SpriteBatch b)
        {
            currentHorizon?.Draw(b, false);
        }
        public static void DrawAfter(SpriteBatch b)
        {
            currentForeground?.Draw(b, true);
        }
        public static void Cleanup()
        {
            currentHorizon = null;
            currentForeground = null;
        }
    }
}
