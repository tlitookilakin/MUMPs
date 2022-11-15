using AeroCore.Backport;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	public class GarbageData
	{
		public float DefaultBaseChance { get; set; } = .2f;
		public List<GarbageItemData> BeforeAll { get; set; }
		public List<GarbageItemData> AfterAll { get; set; }
		public Dictionary<string, GarbageCanData> GarbageCans { get; set; }
	}
	public class GarbageCanData
	{
		public float BaseChance = -1f;
		public List<GarbageItemData> Items;
	}
	public class GarbageItemData : GenericSpawnItemData
	{
		public bool IgnoreBaseChance { get; set; }
		public bool IsMegaSuccess { get; set; }
		public bool IsDoubleMegaSuccess { get; set; }
		public bool AddToInventoryDirectly { get; set; }
	}
}
