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
    class ActionImage
    {
        public static string DirPath = ModEntry.ContentDir + "MapImages" + PathUtilities.PreferredAssetSeparator;
        public static void show(Farmer who, string action, Point _)
        {
            if(Misc.TryLoadAsset<Texture2D>(ModEntry.monitor, ModEntry.helper, DirPath + action, out var tex))
                Game1.activeClickableMenu = new UI.ImageDisplay(tex);
        }
    }
}
