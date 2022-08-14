using AeroCore;
using AeroCore.Generics;
using AeroCore.Models;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [ModInit]
    class ActionImage
    {
        internal static void Init()
        {
            ModEntry.AeroAPI.RegisterAction("SimpleImage", show, 5);
            ModEntry.AeroAPI.RegisterAction("Image", showAdvanced, 5);
        }
        internal static string DirPath = ModEntry.ContentDir + "MapImages" + PathUtilities.PreferredAssetSeparator;
        private static void show(Farmer who, string action, Point tile, GameLocation where)
        {
            if (Misc.TryLoadAsset<Texture2D>(ModEntry.monitor, ModEntry.helper, DirPath + action, out var tex))
                Game1.activeClickableMenu = new UI.ImageDisplay(tex);
            else
                ModEntry.monitor.Log($"COuld not find image asset '{action}' in SimpleImage action @ [{tile.X},{tile.Y}] in '{where.Name}'.");
        }
        private static void showAdvanced(Farmer who, string action, Point tile, GameLocation where)
        {
            if (Assets.Animatons.TryGetValue(action, out var anim))
                Game1.activeClickableMenu = new UI.AdvancedImageDisplay(anim);
            else
                ModEntry.monitor.Log($"Could not find image data for entry '{action}' in Image action @ [{tile.X},{tile.Y}] in '{where.Name}'.");
        }
    }
}
