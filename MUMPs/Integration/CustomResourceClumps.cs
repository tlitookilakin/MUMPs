using AeroCore;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace MUMPs.Integration
{
	public interface IResourceClumpsAPI
	{
		public ResourceClump GetResourceClump(string id, Vector2 tile);
		public bool TryPlaceClump(GameLocation location, string id, Vector2 tile);
		public List<object> GetCustomClumpData();
		public List<string> GetCustomClumpIDs();
	}

	[ModInit(WhenHasMod = "aedenthorn.CustomResourceClumps")]
	internal class CustomResourceClumps
	{
		internal static List<string> knownClumps = new();
		internal static IResourceClumpsAPI API;
		internal static void Init()
		{
			API = ModEntry.helper.ModRegistry.GetApi<IResourceClumpsAPI>("aedenthorn.CustomResourceClumps");
			knownClumps = API.GetCustomClumpIDs();
		}
	}
}
