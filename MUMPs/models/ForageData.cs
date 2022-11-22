using AeroCore;
using AeroCore.Generics;
using AeroCore.Utils;
using Force.DeepCloner;
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
		private readonly Dictionary<string, WeightedArray<ForageItem>> data = new(StringComparer.OrdinalIgnoreCase);
		private bool generated = false;
		internal static readonly Dictionary<string, Item> idCache = new();
		internal static readonly Dictionary<string, Point> clumpSizes = new();

		// key: query string, value: forage pool id
		public Dictionary<string, string> Overrides { get; set; } = new();

		// key: item id, value: metadata
		public Dictionary<string, ForageItem> Items { get; set; } = new();

		public Dictionary<string, WeightedArray<ForageItem>> GetForage()
		{
			if (generated)
				return data;
			data.Clear();
			generated = true;
			Dictionary<string, List<ForageItem>> items = new(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, List<int>> weights = new(StringComparer.OrdinalIgnoreCase);
			foreach(var data in Items.Values)
			{
				string sid = data.ID.Trim();
				switch (data.Type)
				{
					case ForageItem.ForageType.Item:
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
						break;
					case ForageItem.ForageType.Clump:
						if (!int.TryParse(sid, out _) && !CustomResourceClumps.knownClumps.Contains(sid))
						{
							ModEntry.monitor.Log($"Failed to find resource clump with id '{sid}'.", LogLevel.Warn);
							continue;
						}
						if (!clumpSizes.ContainsKey(sid))
						{
							if (ModEntry.helper.ModRegistry.IsLoaded("aedenthorn.CustomResourceClumps"))
							{
								var clump = CustomResourceClumps.API.GetCustomClump(sid, Vector2.Zero);
								clumpSizes.Add(sid, clump is null ? new(2, 2) : new(clump.width.Value, clump.height.Value));
							}
							else
							{
								clumpSizes.Add(sid, new(2, 2));
							}
						}
						if (!ModEntry.AeroAPI.CheckConditions(data.Condition))
							continue;
						break;
				}

				var gt = data.GroundType.Trim();
				if (!items.ContainsKey(gt))
				{
					items.Add(gt, new());
					weights.Add(gt, new());
				}
				items[gt].Add(data.ShallowClone());
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
