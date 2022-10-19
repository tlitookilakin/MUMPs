using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using AeroCore;
using AeroCore.Utils;

namespace MUMPs.Patches
{
	[ModInit]
	internal class SavePersistResourceClumps
	{
		internal static void Init()
		{
			// GCF already does this
			if (ModEntry.helper.ModRegistry.IsLoaded("atravita.GiantCropFertilizer"))
				return;

			ModEntry.harmony.Patch(
				typeof(GameLocation).MethodNamed(nameof(GameLocation.TransferDataFromSavedLocation)),
				postfix: new(typeof(SavePersistResourceClumps), nameof(Postfix))
			);
		}

		// thank you to atravita for this code!
		private static void Postfix(GameLocation __instance, GameLocation l)
		{
			// game handles these two.
			if (__instance is IslandWest || __instance.Name.Equals("Farm", StringComparison.OrdinalIgnoreCase)
				|| __instance.resourceClumps.Count >= l.resourceClumps.Count)
			{
				return;
			}

			// We need to avoid accidentally adding duplicates.
			// Keep track of occupied tiles here.
			HashSet<Vector2> prev = new(l.resourceClumps.Count);

			foreach (var clump in __instance.resourceClumps)
				prev.Add(clump.tile.Value);

			// restore previous giant crops.
			int count = 0;
			foreach (var clump in l.resourceClumps)
			{
				if (clump is GiantCrop crop && prev.Add(crop.tile.Value))
				{
					count++;
					__instance.resourceClumps.Add(crop);
				}
			}

			ModEntry.monitor.Log($"Restored {count} giant crops at {__instance.NameOrUniqueName}");
		}
	}
}
