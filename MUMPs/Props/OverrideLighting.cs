using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MUMPs.Props
{
	[ModInit]
	class OverrideLighting
	{
		internal static void Init()
		{
			ModEntry.OnChangeLocation += UpdateLocation;
		}
		internal static void UpdateLocation(GameLocation where, bool soft)
		{
			where.ignoreOutdoorLighting.Value = where.ignoreOutdoorLighting.Value || where.getMapProperty("IgnoreOutdoorLighting").Length > 0;
		}
	}
}
