using AeroCore;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MUMPs.Props
{
	[ModInit]
	internal class SpawnMonsters
	{
		internal const string KillFlag = "tlitookilakin.mumps.spawnedMonster";
		private static readonly ConditionalWeakTable<GameLocation, RefInt> spawnedCount = new();
		internal static void Init()
		{
			ModEntry.OnChangeLocation += EnterLocation;
			ModEntry.helper.Events.GameLoop.DayEnding += OnDayEnd;
			ModEntry.helper.Events.World.NpcListChanged += OnNpcsChanged;
		}
		private static void EnterLocation(GameLocation where)
		{
			var data = where.MapPropertyArray("MonsterSpawns");
			if (data.Length < 2 || !int.TryParse(data[1], out int count))
				return;
			if (!spawnedCount.TryGetValue(where, out var spawned))
				spawnedCount.Add(where, spawned = new(0));
			if (spawned.Value >= count)
				return;
			count -= spawned.Value;
			var spawner = GetOverride(data[0], where)?.GetSpawnTable();
			if (spawner is null)
				return;
			Rectangle region = new(0, 0, where.Map.DisplayHeight / 64, where.Map.DisplayWidth / 64);
			int tries = 0;
			while(count > 0 && tries < (count * 3))
			{
				if (spawner.Choose().TrySpawnAt(where, Game1.random.Next(region).ToVector2()))
				{
					spawned.Value++;
					count--;
				}
				tries++;
			}
		}
		private static void OnDayEnd(object _, DayEndingEventArgs ev)
		{
			spawnedCount.Clear();
			if (!Context.IsMainPlayer)
				return;
			foreach(var loc in Game1.locations)
				for(int i = loc.characters.Count; i > 0; i--)
					if (loc.characters[i - 1].modData.ContainsKey(KillFlag))
						loc.characters.RemoveAt(i - 1);
		}
		private static void OnNpcsChanged(object _, NpcListChangedEventArgs ev)
		{
			if (!spawnedCount.TryGetValue(ev.Location, out var count))
				return;
			foreach (var npc in ev.Removed)
				if (npc.modData.ContainsKey(KillFlag))
					count.Value--;
		}

		private static SpawnData GetOverride(string name, GameLocation loc)
		{
			var asset = Assets.Spawns;
			if (!asset.TryGetValue(name, out var data))
			{
				ModEntry.monitor.Log($"Monster spawn data with id '{name}' does not exist! @ {loc.mapPath.Value}", LogLevel.Warn);
				return null;
			}
			HashSet<string> history = new();
			var currentID = name;
			var lastID = "";
			while (currentID != lastID)
			{
				lastID = currentID;
				if (!history.Add(currentID))
				{
					StringBuilder sb = new();
					sb.Append("Cyclic spawn table overrides encountered at '").Append(loc.mapPath.Value).Append("': '");
					foreach(var i in history)
						sb.Append(i).Append("' > '");
					sb.Append(currentID).Append('\'');
					ModEntry.monitor.Log(sb.ToString(), LogLevel.Warn);
					return null;
				}
				foreach ((var id, var condition) in data.Overrides)
				{
					if (ModEntry.AeroAPI.CheckConditions(condition, target_location: loc))
					{
						if (asset.TryGetValue(id, out var next))
						{
							currentID = id;
							data = next;
							break;
						}
						else
						{
							ModEntry.monitor.Log($"Monster spawn data with id '{name}' does not exist! @ {loc.mapPath.Value}", LogLevel.Warn);
						}
					}
				}
			}
			return data;
		}

		private class RefInt
		{
			internal int Value;
			internal RefInt(int val)
			{
				Value = val;
			}
		}
	}
}
