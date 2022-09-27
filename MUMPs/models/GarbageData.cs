using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	public class GarbageData
	{
		public float DefaultBaseChance = .2f;
		public List<GarbageItemData> BeforeAll;
		public List<GarbageItemData> AfterAll;
		public Dictionary<string, GarbageCanData> GarbageCans;
	}
	public class GarbageCanData
	{
		public float BaseChance = -1f;
		public List<GarbageItemData> Items;
	}
	public class GarbageItemData
	{
		public string ID;
		public string Condition;
		public string ItemId;
		public List<string> RandomItemId;
		public int Stack = 1;
		public int Quality;
		public bool IsRecipe;
		public bool IgnoreBaseChance;
		public bool IsMegaSuccess;
		public bool IsDoubleMegaSuccess;
		public bool AddToInventoryDirectly;
	}
}
