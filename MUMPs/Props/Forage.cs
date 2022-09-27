using AeroCore.Generics;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MUMPs.Integration;
using MUMPs.models;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using SObject = StardewValley.Object;

namespace MUMPs.Props
{
	[HarmonyPatch]
	internal class Forage
	{
		[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.spawnObjects))]
		[HarmonyPrefix]
		internal static bool spawnForage(GameLocation __instance)
		{
			var defval = __instance.getMapProperty("DefaultForagePool");
			var defsplit = defval?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
			bool skip = defsplit.Length > 2 && defsplit[2].StartsWith("T", StringComparison.OrdinalIgnoreCase);
			if (defsplit.Length > 1 && int.TryParse(defsplit[1], out int count))
			{
				var size = __instance.Map.GetLayer("Back").LayerSize;
				var forage = GetForageDataAt(defsplit[0], __instance);
				if (forage is null)
					ModEntry.monitor.Log($"No forage pool data found for '{defsplit[0]}' in location '{__instance.Name}'.");
				else
					SpawnInLocationArea(__instance, new(0, 0, size.Width, size.Height), count, forage);
			}
			SpawnInLocation(__instance);
			return !skip;
		}

		[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.spawnObjects))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> defaultNamePatch(IEnumerable<CodeInstruction> source)
		{
			var field = typeof(GameLocation).FieldNamed(nameof(GameLocation.name));
			var method = typeof(Forage).MethodNamed(nameof(SwapForageName));

			bool marked = false;
			foreach(var code in source)
			{
				yield return code;
				if (code.opcode == OpCodes.Ldfld && field.Equals(code.operand))
					marked = true;
				else if (marked && code.opcode == OpCodes.Call) 
				{
					yield return new(OpCodes.Ldarg_0);
					yield return new(OpCodes.Call, method);
					marked = false;
				} else
				{
					marked = false;
				}
			}
		}

		private static string SwapForageName(string original, GameLocation loc)
			=> loc.Map.Properties.TryGetValue("DefaultForageArea", out var val) && val is not null ? val.ToString() : original;

		internal static void SpawnInLocation(GameLocation loc)
		{
			if (loc is null)
				return;
			var split = loc.getMapProperty("ForagePoolCorners")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split is not null)
			{
				for (int i = 0; i < split.Length - 5; i += 6)
				{
					if (!split.FromCorners(out var rect, i) || !int.TryParse(split[i + 5], out int count))
						continue;
					string current = split[i + 4];
					var forage = GetForageDataAt(current, loc);
					if (forage is null)
						ModEntry.monitor.Log($"No forage pool data found for '{current}' in location '{loc.Name}'.");
					else
						SpawnInLocationArea(loc, rect, count, forage);
				}
			}
			split = loc.getMapProperty("ForageAreaCorners")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split is not null)
			{
				var data = Game1.content.Load<Dictionary<string, string>>("Data/Locations");
				var r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
				for (int i = 0; i < split.Length - 5; i += 5)
				{
					if (!split.FromCorners(out var rect, i) ||
						!data.TryGetValue(split[i + 4], out var entry) ||
						!int.TryParse(split[i + 5], out int count))
						continue;

					var dsplit = entry.GetChunk('/', Utility.getSeasonNumber(loc.GetSeasonForLocation()))
						.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					for (int n = 0; n < count; n++)
					{
						int which = Game1.random.Next(dsplit.Length / 2) * 2;
						if (!dsplit[which].TryGetItem(out var item) || !double.TryParse(dsplit[which + 1], out var chance) ||
							r.NextDouble() >= chance)
							continue;
						SpawnAt(loc, new(r.Next(rect.Width) + rect.X, r.Next(rect.Height) + rect.Y), item);
					}
				}
			}
		}
		private static ForageData GetForageDataAt(string initial, GameLocation where)
		{
			List<string> history = new();
			ForageData forage = null;
			bool stop = false;
			while (!stop && Assets.Forage.TryGetValue(initial, out forage))
			{
				stop = true;
				foreach ((var over, var condition) in forage.Overrides)
				{
					if (ModEntry.AeroAPI.CheckConditions(condition, target_location: where))
					{
						initial = over;
						stop = false;
						break;
					}
				}
				if (history.Contains(initial))
				{
					StringBuilder sb = new();
					sb.Append("Cyclic forage table overrides encountered in location '");
					sb.Append(where.Name).AppendLine("' @ path:");
					for (int ii = 0; ii < history.Count; ii++)
						sb.Append(history[ii]).Append(" > ");
					sb.Append(initial);
					ModEntry.monitor.Log(sb.ToString(), LogLevel.Warn);
					break;
				}
			}
			return forage;
		}
		private static void SpawnInLocationArea(GameLocation loc, Rectangle region, int attempts, ForageData forage)
		{
			var pools = forage.GetForage();
			for(int i = 0; i < attempts; i++)
			{
				Vector2 tile = new(Game1.random.Next(region.Left, region.Right), Game1.random.Next(region.Top, region.Bottom));
				if (!CanSpawnAt(loc, tile))
					continue;
				var ground = loc.doesTileHaveProperty((int)tile.X, (int)tile.Y, "type", "back");
				WeightedArray<string> pool;
				if (ground is null || ground == string.Empty)
					pool = pools.Values.ElementAt(Game1.random.Next(pools.Count));
				else if (!pools.TryGetValue(ground, out pool))
					continue;
				var what = pool.Choose();
				if (what.StartsWith("(RC)"))
				{
					SpawnClump(loc, tile, what[4..]);
				} else
				{
					SpawnAt(loc, tile, ForageData.idCache[what]);
				}
			}
		}
		internal static void SpawnAt(GameLocation loc, Vector2 pos, Item what)
		{
			if (CanSpawnAt(loc, pos))
			{
				var obj = ModEntry.AeroAPI.WrapItem(what, true);
				loc.objects[pos] = obj;
				obj.TileLocation = pos;
				obj.IsSpawnedObject = true;
				obj.CanBeGrabbed = true;
			}
		}
		internal static void SpawnClump(GameLocation loc, Vector2 pos, string id)
		{
			var size = ForageData.clumpSizes[id];

			var tile = new Vector2(pos.X, pos.Y);
			for (int x = 0; x < size.X; x++)
			{
				for (int y = 0; y < size.Y; y++)
				{
					if (loc.Objects.ContainsKey(tile) ||
						loc.doesEitherTileOrTileIndexPropertyEqual((int)pos.X, (int)pos.Y, "Spawnable", "Back", "F") ||
						!loc.isTileLocationTotallyClearAndPlaceable(pos)
					)
						return;
					tile.Y++;
				}
				tile.X++;
			}
			if (!int.TryParse(id, out int index))
				CustomResourceClumps.API.TryPlaceClump(loc, id, pos);
			else
				loc.resourceClumps.Add(new(index, 2, 2, pos));
		}
		internal static bool CanSpawnAt(GameLocation loc, Vector2 pos)
			=> !loc.Objects.ContainsKey(pos) && !loc.doesEitherTileOrTileIndexPropertyEqual((int)pos.X, (int)pos.Y, "Spawnable", "Back", "F") &&
			loc.isTileLocationTotallyClearAndPlaceable(pos) && loc.getTileIndexAt((int)pos.X, (int)pos.Y, "AlwaysFront") == -1 && 
			loc.getTileIndexAt((int)pos.X, (int)pos.Y, "Front") == -1 && !loc.isBehindBush(pos) && !loc.isBehindTree(pos);
	}
}
