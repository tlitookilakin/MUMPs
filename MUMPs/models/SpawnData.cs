using AeroCore;
using AeroCore.Generics;
using System.Collections.Generic;

namespace MUMPs.models
{
	[ModInit]
	public class SpawnData
	{
		public Dictionary<string, string> Overrides { get; set; }
		public Dictionary<string, SpawnDataItem> Items { get; set; }

		private WeightedArray<SpawnDataItem> data;
		public WeightedArray<SpawnDataItem> GetSpawnTable()
		{
			if (data is null)
			{
				var weights = new int[Items.Count];
				var items = new SpawnDataItem[Items.Count];
				int i = 0;
				foreach(var item in Items.Values)
				{
					weights[i] = item.Weight;
					items[i] = item;
					i++;
				}
				data = new(items, weights);
			}
			return data;
		}
	}
}
