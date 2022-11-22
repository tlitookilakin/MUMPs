using AeroCore.Backport;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
	public class ArtifactData
	{
		public List<Artifact> PriorityDrops { get; set; }
		public List<Artifact> FallbackDrops { get; set; }
		public Dictionary<string, Dictionary<string, Artifact>> Locations { get; set; }
	}
	public class Artifact : GenericSpawnItemData
	{
		public bool ApplyGenerousEnchant { get; set; } = true;
		public bool OneDebrisPerDrop { get; set; } = true;
		public bool ContinueOnDrop { get; set; } = false;
		public float Chance { get; set; }
		public int Precedence { get; set; }
	}
	// TODO add artifact spot patch
}
