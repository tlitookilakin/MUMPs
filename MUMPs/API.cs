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
    class API : IMumpsAPI
    {
        public event Action<object, float> OnLighting;

        internal void InvokeLighting(float intensity)
        {
            OnLighting?.Invoke(Game1.spriteBatch, intensity);
        }
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
        public bool TryParseColor(string str, out Color color)
        {
            return Utils.TryParseColor(str, out color);
        }
        public void PlaceItemAt(GameLocation location, Vector2 tile, string type, string id)
        {
            Props.SpawnObject.GenerateAt(location, tile, type, id);
        }
        public void RegisterAction(string id, Action<Farmer, string, Point> action, bool isInspect = false)
        {
            Utils.AddAction(id, isInspect, action);
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
            Utils.BroadcastReloadRequest(location.Name);
        }
        public void ReloadScreen(int id = -1)
        {
            if (id == -1)
                Events.reloadScreen.Value = true;
            else
                Events.reloadScreen.SetValueForScreen(id, true);
        }
        public void UnregisterAction(string id)
        {
            Utils.RemoveAction(id);
        }
        public void WarpToTempMap(Farmer who, string path)
        {
            Utils.warpToTempMap(path, who);
        }
    }
}
