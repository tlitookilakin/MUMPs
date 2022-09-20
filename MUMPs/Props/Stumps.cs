using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace MUMPs.Props
{
	class Stumps
	{
		private static readonly Point stumpArea = new(2, 2);
		public static void SpawnMapStumps(GameLocation location)
		{
			string[] stumpList = Maps.MapPropertyArray(location, "Stumps");
			if(stumpList.Length > 0)
				ModEntry.monitor.Log($"Adding stumps to {location.Name}.", LogLevel.Trace);
			for(int i = 0; i + 2 < stumpList.Length; i += 3)
				if (stumpList.ToPoint(out Point pos, i) && location.isAreaClear(new Rectangle(pos, stumpArea))) //x, y, unused
					location.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, pos.ToVector2());
					//will not be saved in most locations, but that's fine because they are regenerated at day start anyways
		}
	}
}
