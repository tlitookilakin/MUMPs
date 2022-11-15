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
	}

	public class FishArea
	{
		public Rectangle? Position { get; set; }
		public string DisplayName { get; set; }
		public List<string> FishTypes { get; set; }
		public List<string> CrabPotFishTypes { get; set; }
		public float CrabPotJunkChance { get; set; }
		public int LegacyAreaIndex { get; set; } = -1;
		public List<string> IncludeFishFrom { get; set; }

		public bool IsInside(Point tile, int zone)
			=> zone == LegacyAreaIndex || (Position?.Contains(tile) ?? true);
	}

	public class Artifact : GenericSpawnItemData
	{
		public bool ApplyGenerousEnchant { get; set; } = true;
		public bool OneDebrisPerDrop { get; set; } = true;
		public bool ContinueOnDrop { get; set; } = false;
		public float Chance { get; set; }
		public int Precedence { get; set; }
	}
}
