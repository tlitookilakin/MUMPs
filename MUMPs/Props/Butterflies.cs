using AeroCore;
using HarmonyLib;
using StardewValley;
using StardewValley.BellsAndWhistles;
namespace MUMPs.Props
{
	[ModInit]
	class Butterflies
	{
		internal static void Init()
		{
			ModEntry.OnChangeLocation += EnterLocation;
		}
		private static void EnterLocation(GameLocation location)
		{
			if (!int.TryParse(location.getMapProperty("Butterflies"), out int count))
				return;

			bool isIsland = location.getMapProperty("LocationContext") == "Island" || location.Name.StartsWith("Island");

			location.instantiateCrittersList();
			for(int i = 0; i < count; i++)
				location.addCritter(new Butterfly(location.getRandomTile(), isIsland).setStayInbounds(true));
		}
	}
}
