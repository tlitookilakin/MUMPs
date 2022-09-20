using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MUMPs.Props
{
	[HarmonyPatch]
	[ModInit]
	internal class ActionMinecart
	{
		private static readonly int[] tiles = {1080, 1081, 958};

		internal static void Init()
		{
			ModEntry.AeroAPI.RegisterAction("MinecartTransport", DoAction);
			
			HarmonyMethod patch = new(typeof(ActionMinecart).MethodNamed(nameof(MinecartOverride)));
			ModEntry.harmony.Patch(typeof(BusStop).DeclaredMethod("checkAction"), patch);
			ModEntry.harmony.Patch(typeof(Mountain).DeclaredMethod("checkAction"), patch);
			ModEntry.harmony.Patch(typeof(Town).DeclaredMethod("checkAction"), patch);
		}

		private static bool MinecartOverride(GameLocation __instance, xTile.Dimensions.Location tileLocation, Farmer who, ref bool __result)
		{
			if (!tiles.Contains(__instance.getTileIndexAt(new(tileLocation.X, tileLocation.Y), "Buildings")))
				return true;
			__result = __instance.performAction("MinecartTransport", who, tileLocation);
			return false;
		}

		private static void DoAction(Farmer who, string what, Point tile, GameLocation where)
		{
			ModEntry.monitor.Log($"minecart activated @ {where.NameOrUniqueName} : [{tile.X},{tile.Y}] : '{what}'");
			var data = Assets.MiscGameData;
			if (data.MineCartCondition is null || ModEntry.AeroAPI.CheckConditions(data.MineCartCondition))
			{
				string network = what is null || what.Length == 0 ? null : what;
				List<KeyValuePair<string, string>> destinations = new();

				foreach ((var id, var dest) in data.MineCartDestinations)
				{
					if (dest.Network == network && 
						(dest.Location != where.NameOrUniqueName || Math.Abs(dest.Tile.X - tile.X) >= 8 || Math.Abs(dest.Tile.Y - tile.Y) >= 8) &&
						(dest.Condition is null || ModEntry.AeroAPI.CheckConditions(dest.Condition))
					)
						destinations.Add(new(ModEntry.AeroAPI.ParseTokenText(dest.DisplayName), id));
				}
				where.ShowPagedResponses(Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), destinations, OnCartWarp);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
			}
		}
		private static void OnCartWarp(Farmer who, string what)
		{
			ModEntry.monitor.Log(what);
			var data = Assets.MiscGameData;
			if (data.MineCartDestinations != null && data.MineCartDestinations.TryGetValue(what, out var dest))
			{
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				int dir = dest.Direction?.ToUpperInvariant() switch
				{
					"UP" => 0,
					"DOWN" => 2,
					"LEFT" => 3,
					"RIGHT" => 1,
					_ => -1
				};
				Game1.warpFarmer(dest.Location, dest.Tile.X, dest.Tile.Y, dir);
				if (Game1.getMusicTrackName() == "springtown")
				{
					Game1.changeMusicTrack("none");
				}
			}
		}
	}
}
