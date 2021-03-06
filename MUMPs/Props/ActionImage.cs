using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace MUMPs.Props
{
    [ModInit]
    class ActionImage
    {
        internal static void Init()
        {
            ModEntry.AeroAPI.RegisterAction("Image", show, 5);
        }
        internal static string DirPath = ModEntry.ContentDir + "MapImages" + PathUtilities.PreferredAssetSeparator;
        private static void show(Farmer who, string action, Point _, GameLocation where)
        {
            if(Misc.TryLoadAsset<Texture2D>(ModEntry.monitor, ModEntry.helper, DirPath + action, out var tex))
                Game1.activeClickableMenu = new UI.ImageDisplay(tex);
        }
    }
}
