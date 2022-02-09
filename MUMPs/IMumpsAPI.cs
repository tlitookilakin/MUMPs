using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs
{
    public interface IMumpsAPI
    {
        public event Action<object, float> OnLighting;
        public void DrawInspectBubble(Point position);
        public void ReloadLocation(GameLocation location);
        public void ReloadScreen(int id = -1);
        public void ReloadAllScreens();
        public void DisplayImage(string assetPath);
        public void RegisterAction(string id, Action<Farmer, string, Point> action, bool isInspect = false);
        public void UnregisterAction(string id);
        public void PlaceItemAt(GameLocation location, Vector2 tile, string type, string id);
        //public void PlaceItemAt(GameLocation location, Vector2 tile, string uniqueID);
        public void WarpToTempMap(Farmer who, string path);
        public bool TryParseColor(string str, out Color color);
    }
}
