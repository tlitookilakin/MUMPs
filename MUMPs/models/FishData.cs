using AeroCore.Backport;
using Microsoft.Xna.Framework;
using MUMPs.Props;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.models
{
	public class FishData : GenericSpawnItemData
	{
		public float Chance { get; set; }
		public string Season { get; set; }
		public string FishAreaId { get; set; }
		public Rectangle? BobberPosition { get; set; }
		public Rectangle? PlayerPosition { get; set; }
		public int MinFishingLevel { get; set; }
		public int MinDistanceFromShore { get; set; }
		public int MaxDistanceFromShore { get; set; } = -1;
		public bool ApplyDailyLuck { get; set; } = true;
		public float CuriosityLureBuff { get; set; } = -1f;
		public int CatchLimit { get; set; } = -1;
		public string SetFlagOnCatch { get; set; }
		public bool RequireMagicBait { get; set; }
		public int Precedence { get; set; }
		public bool CheckFishingRequirements { get; set; } = true;

		public bool CanCatchFish(int Depth, Vector2 tile, Farmer who, int bait, GameLocation where, string season)
		{
			double chance = who?.CurrentTool is FishingRod rod && rod.getBobberAttachmentIndex() == 856 ? Chance + CuriosityLureBuff : Chance;
			// TODO use actual probability calcs
			chance = ApplyDailyLuck ? chance + 0f : chance;
			if (Depth < MinDistanceFromShore || (RequireMagicBait && bait != 908) ||
				(!BobberPosition?.Contains(tile) ?? false) || (!PlayerPosition?.Contains(who.getTileLocationPoint()) ?? false) ||
				Game1.random.NextDouble() >= chance || !ModEntry.AeroAPI.CheckConditions(Condition, null, who, null, where))
				return false;

			int maxDepth = MaxDistanceFromShore;
			int minLevel = MinFishingLevel;
			string target_season = Season;

			string id = ItemId;
			string oid = id.StartsWith("(O)") ? id[3..] : id;
			var index = 0;
			bool checkRequirements = CheckFishingRequirements && bait != 908 && int.TryParse(oid, out index);
			if (index > 0 && CatchLimit >= 0)
				if (CatchLimit == 0 || (who.fishCaught.TryGetValue(index, out var caught) && caught[0] >= CatchLimit))
					return false;
			if (checkRequirements) {
				string[] fishData = GetFishData(index).Split('/');
				if (fishData.Length > 0)
				{
					if (fishData[7] != "both")
						if ((fishData[7] == "sunny" && Game1.IsRainingHere(where)) ||
							(fishData[7] == "rainy" && !Game1.IsRainingHere(where)))
							return false;

					var time = fishData[5].Split(' ');
					if (time.Length < 2 || !int.TryParse(time[0], out var start) || !int.TryParse(time[1], out var end) || Game1.timeOfDay < start || Game1.timeOfDay > end)
						return false;
					target_season = fishData[6];
					maxDepth = int.TryParse(fishData[9], out var mdepth) ? mdepth : maxDepth;
				}
			}
			if (target_season.Length > 0 && season.Length > 0 && !target_season.Split(' ').Contains(season))
				return false;
			return Depth <= maxDepth && who.FishingLevel >= minLevel;
		}
		public override Item Spawn(Farmer who = null, GameLocation where = null)
		{
			var ret = base.Spawn(who, where);
			if (ret is not null && SetFlagOnCatch is not null)
				ret.modData["tlitoo.mumps.flagOnCatch"] = SetFlagOnCatch;
			return ret;
		}
		private static string GetFishData(int which)
			=> ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Fish").TryGetValue(which, out var ret) ? ret : "";
	}
}
