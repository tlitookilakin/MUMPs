using AeroCore;
using StardewValley;
using System;

namespace MUMPs.Props
{
	[ModInit]
	internal class EntryMail
	{
		internal static void Init()
		{
			ModEntry.OnChangeLocation += Enter;
		}
		private static void Enter(GameLocation loc)
		{
			var prop = loc.getMapProperty("EntryMail");
			if (prop is null)
				return;
			var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 0)
				return;
			bool noLetter = split.Length > 1 && !split[0].Equals("T", StringComparison.OrdinalIgnoreCase);
			bool forAll = split.Length > 2 && split[1].Equals("T", StringComparison.OrdinalIgnoreCase);
			int ind = split.Length >= 3 ? 2 : split.Length - 1;
			Game1.addMail(split[ind], noLetter, forAll);
		}
	}
}
