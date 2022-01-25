using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(data.Length >= 3 && float.TryParse(data[0], out float radius) && float.TryParse(data[1], out float fade) && Utils.TryParseColor(data[2], out Color col))
            {
                if (data.Length >= 4)
                    if (float.TryParse(data[3], out float alpha))
                        col *= alpha;
                fog.Value = new(radius, fade, col);
            }
        }
        public static void Draw()
        {
            fog.Value?.Draw(Game1.player.position);
        }
    }
}
