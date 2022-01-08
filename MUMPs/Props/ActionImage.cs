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
        public static void show(Farmer who, string action)
        {
            try
            {
                Game1.activeClickableMenu = new UI.ImageDisplay(ModEntry.helper.Content.Load<Texture2D>(DirPath + action, ContentSource.GameContent));
            } catch(ContentLoadException e)
            {
                ModEntry.monitor.Log("Failed to load display image '" + DirPath + action + "' from game content:\n" + e.Message, LogLevel.Warn);
            }
        }
    }
}
