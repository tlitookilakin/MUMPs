
namespace MUMPs.models
{
	public class ForageItem
	{
		public enum ForageType {Item, Clump, Ore, GiantCrop, Crop}

		public string Condition { get; set; }
		public string GroundType { get; set; }
		public int Weight { get; set; } = 10;
		public ForageType Type { get; set; } = ForageType.Item;
		public string ID { get; set; }
	}
}
