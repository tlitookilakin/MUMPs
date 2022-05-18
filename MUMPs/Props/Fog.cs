using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace MUMPs.Props
{
    class Fog
    {
        public static PerScreen<models.FogModel> fog = new();

        public static void ChangeLocation(GameLocation loc)
        {
            fog.Value = null;

            string prop = loc.getMapProperty("Fog");
            if (prop == null)
                return;

            string[] data = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if(data.Length >= 2 && float.TryParse(data[0], out float radius) && Strings.TryParseColor(data[1], out Color col))
            {
                if (data.Length >= 3)
                    if (float.TryParse(data[2], out float alpha))
                        col *= alpha;
                fog.Value = new(radius, col);
            }
        }
        public static void Draw(SpriteBatch b)
        {
            fog.Value?.Draw(b);
        }
    }
}
