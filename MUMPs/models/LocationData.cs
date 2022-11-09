using AeroCore.Backport;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	public class LocationData
	{
		public List<Artifact> PriorityArtifactSpotDrops { get; set; }
		public List<Artifact> FallbackArtifactSpotDrops { get; set; }
		public Dictionary<string, LocationItem> Locations { get; set; }
	}
	
	public class LocationItem
	{
		public string DisplayName { get; set; }
		public List<Artifact> ArtifactSpots { get; set; }
		public Dictionary<string, FishArea> FishAreas { get; set; }
		public List<FishData> Fish { get; set; }
		public List<object> Forage { get; set; }
	}

	public class FishArea
	{
		public Rectangle Position { get; set; }
		public string DisplayName { get; set; }
		public List<string> FishTypes { get; set; }
		public List<string> CrabPotFishTypes { get; set; }
		public float CrabPotJunkChance { get; set; }
	}

	public class Artifact : GenericSpawnItemData
	{
		public bool ApplyGenerousEnchant { get; set; } = true;
		public bool OneDebrisPerDrop { get; set; } = true;
		public bool ContinueOnDrop { get; set; } = false;
		public float Chance { get; set; }
		public int Precedence { get; set; }
	}
	public class FishData : GenericSpawnItemData
	{
		public float Chance { get; set; }
		public string Season { get; set; }
		public string FishAreaId { get; set; }
		public Point? BobberPosition { get; set; }
		public Point? PlayerPosition { get; set; }
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
	}
}
