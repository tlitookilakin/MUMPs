﻿using System;
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
            if(name.Length == 0 || name.ToLower() == "default")
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
            currentHorizon = getTemplate(loc.getMapProperty("horizon").Trim());
            currentForeground = getTemplate(loc.getMapProperty("foreground").Trim());
        }
        public static void DrawBefore(SpriteBatch b)
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
