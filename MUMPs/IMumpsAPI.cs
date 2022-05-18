using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs
{
    public interface IMumpsAPI
    {
        public void DrawInspectBubble(Point position);
        public void ReloadLocation(GameLocation location);
        public void ReloadScreen(int id = -1);
        public void ReloadAllScreens();
        public void DisplayImage(string assetPath);
        public void PlaceItemAt(GameLocation location, Vector2 tile, string type, string id);
        //public void PlaceItemAt(GameLocation location, Vector2 tile, string uniqueID);
        public void WarpToTempMap(Farmer who, string path);
    }
}
