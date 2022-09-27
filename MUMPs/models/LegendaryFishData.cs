using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	internal class LegendaryFishData
	{
		public string Anchor { get; set; }
		public int Fish { get; set; }
		public int Family { get; set; }
		public int MinLevel { get; set; }
		public int MinDepth { get; set; }
		public double Chance { get; set; }
		public double Curiosity { get; set; }
		public string Weather { get; set; }
		public List<string> Seasons { get; set; }
		public Rectangle Region { set; get; }

		internal bool CanCatchHere(int Depth, Vector2 tile, Farmer who, int bait, GameLocation where)
		{
			string season = where.GetSeasonForLocation();
			if (season is null || season.Length < 1)
				season = Game1.currentSeason;
			double chance = bait == 856 ? Chance + Curiosity : Chance;
			if (Depth < MinDepth || who.FishingLevel < MinLevel || Game1.random.NextDouble() >= chance || !Seasons.Contains(season))
				return false;
			var weather = Weather == "default" ? GetFishData(GetFishIndex()).GetChunk(' ', 7) : Weather;
			if (weather != "both")
				if ((weather == "sunny" && Game1.IsRainingHere(where)) || 
					(weather == "rainy" && !Game1.IsRainingHere(where)))
					return false;
			return Region.Contains(Anchor == "player" ? who.getTileLocation() : tile);
		}
		internal int GetFishIndex()
			=> Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY") ? Fish : Family;
		private string GetFishData(int which)
			=> ModEntry.helper.GameContent.Load<Dictionary<int, string>>("Data/Fish")[which];
	}
}
