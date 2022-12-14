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
		private static void Enter(GameLocation loc, bool soft)
		{
			var prop = loc.getMapProperty("EntryMail");
			if (prop is null)
				return;
			var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 0)
				return;
			bool noLetter = split.Length > 1 && !split[1].Equals("T", StringComparison.OrdinalIgnoreCase);
			bool forAll = split.Length > 2 && split[2].Equals("T", StringComparison.OrdinalIgnoreCase);
			Game1.addMail(split[0], noLetter, forAll);
		}
	}
}
