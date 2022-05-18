using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs
{
    public class API : IMumpsAPI
    {
        public void DisplayImage(string assetPath)
        {
            if (Game1.activeClickableMenu == null) {
                try
                {
                    Game1.activeClickableMenu = new UI.ImageDisplay(ModEntry.helper.Content.Load<Texture2D>(Props.ActionImage.DirPath + assetPath, ContentSource.GameContent));
                }
                catch (ContentLoadException e)
                {
                    ModEntry.monitor.Log("Failed to load display image '" + Props.ActionImage.DirPath + assetPath + "' from game content:\n" + e.Message, LogLevel.Warn);
                }
            }
        }
        public void DrawInspectBubble(Point position)
        {
            throw new NotImplementedException();
        }
        public void PlaceItemAt(GameLocation location, Vector2 tile, string type, string id)
        {
            Props.SpawnObject.GenerateAt(location, tile, type, id);
        }
        public void ReloadAllScreens()
        {
            foreach((int id, bool _) in Events.reloadScreen.GetActiveValues().ToArray())
            {
                Events.reloadScreen.SetValueForScreen(id, true);
            }
        }
        public void ReloadLocation(GameLocation location)
        {
            Utility.BroadcastReloadRequest(location.Name);
        }
        public void ReloadScreen(int id = -1)
        {
            if (id == -1)
                Events.reloadScreen.Value = true;
            else
                Events.reloadScreen.SetValueForScreen(id, true);
        }
        public void WarpToTempMap(Farmer who, string path)
        {
            Utility.warpToTempMap(path, who);
        }
    }
}
