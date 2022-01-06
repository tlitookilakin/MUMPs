using HarmonyLib;
using StardewValley;
using StardewValley.BellsAndWhistles;
namespace MUMPs.Props
{
    [HarmonyPatch(typeof(GameLocation),"resetLocalState")]
    class Butterflies
    {
        public static void Postfix(GameLocation __instance)
        {
            EnterLocation(__instance);
        }
        public static void EnterLocation(GameLocation location)
        {
            if (!int.TryParse(location.getMapProperty("Butterflies"), out int count))
                return;

            bool isIsland = location.getMapProperty("LocationContext") == "Island" || location.Name.StartsWith("Island");

            location.instantiateCrittersList();
            for(int i = 0; i < count; i++)
            {
                location.addCritter(new Butterfly(location.getRandomTile(), isIsland).setStayInbounds(true));
            }
        }
    }
}
