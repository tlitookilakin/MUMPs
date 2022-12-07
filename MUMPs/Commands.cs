using AeroCore;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs
{
	[ModInit]
	internal class Commands
	{
		internal static void Init()
		{
			ModEntry.helper.ConsoleCommands.Add("mumps_rebuild", "Regenerate spawns in the current location", RebuildLocation);
		}
		private static void RebuildLocation(string cmd, string[] args)
		{
			Game1.currentLocation.modData.Remove("tlitookilakin.mumps.generatedObjects");
			Props.SpawnCrops.Spawn(Game1.currentLocation, true);
			Props.SpawnObject.Generate(Game1.currentLocation, false);
		}
	}
}
