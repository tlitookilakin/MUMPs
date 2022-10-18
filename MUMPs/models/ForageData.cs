using AeroCore;
using AeroCore.Generics;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using MUMPs.Integration;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	[ModInit]
	public class ForageData
	{
		private readonly Dictionary<string, WeightedArray<string>> data = new(StringComparer.OrdinalIgnoreCase);
		private bool generated = false;
		internal static readonly Dictionary<string, Item> idCache = new();
		internal static readonly Dictionary<string, Point> clumpSizes = new();

		// key: query string, value: forage pool id
		public Dictionary<string, string> Overrides { get; set; } = new();

		// key: item id, value: metadata
		public Dictionary<string, ForageItem> Items { get; set; } = new();

		public Dictionary<string, WeightedArray<string>> GetForage()
		{
			if (generated)
				return data;
			data.Clear();
			generated = true;
			Dictionary<string, List<string>> items = new(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, List<int>> weights = new(StringComparer.OrdinalIgnoreCase);
			foreach((var id, var data) in Items)
			{
				string sid = id.Trim();
				if (sid.StartsWith("(RC)"))
				{
					var rcid = sid[4..];
					if (!int.TryParse(rcid, out _) && !CustomResourceClumps.knownClumps.Contains(rcid))
					{
						ModEntry.monitor.Log($"Failed to find resource clump with id '{rcid}'.", LogLevel.Warn);
						continue;
					}
					if (!clumpSizes.ContainsKey(sid))
					{
						if (ModEntry.helper.ModRegistry.IsLoaded("aedenthorn.CustomResourceClumps"))
						{
							var clump = CustomResourceClumps.API.GetCustomClump(rcid, Vector2.Zero);
							clumpSizes.Add(rcid, clump is null ? new(2, 2) : new(clump.width.Value, clump.height.Value));
						} else
						{
							clumpSizes.Add(rcid, new(2, 2));
						}
					}
					if (!ModEntry.AeroAPI.CheckConditions(data.Condition))
						continue;
				}
				else
				{
					if (!idCache.TryGetValue(sid, out var obj))
						if (sid.TryGetItem(out var item))
							idCache.Add(sid, obj = item);
						else
							idCache.Add(sid, obj = null);

					if (obj is null)
					{
						ModEntry.monitor.Log($"Failed to find item with id '{sid}'.", LogLevel.Warn);
						continue;
					}

					if (!ModEntry.AeroAPI.CheckConditions(data.Condition, target_item: obj))
						continue;
				}

				var gt = data.GroundType.Trim();
				if (!items.ContainsKey(gt))
				{
					items.Add(gt, new());
					weights.Add(gt, new());
				}
				items[gt].Add(sid);
				weights[gt].Add(data.Weight);
			}
			foreach(var ground in items.Keys)
				data.Add(ground, new(items[ground], weights[ground]));
			return data;
		}
		internal static void Init()
		{
			ModEntry.helper.Events.GameLoop.DayEnding += (s, e) => NewDay();
		}
		private static void NewDay()
		{
			idCache.Clear();
			foreach (var obj in Assets.Forage.Values)
				obj.generated = false;
		}
	}
}
