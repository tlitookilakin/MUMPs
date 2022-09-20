using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	internal class MiscGameData
	{
		public Dictionary<string, MinecartLocation> MineCartDestinations { get; set; } = new();
		public string MineCartCondition { get; set; } = string.Empty;
	}
}
