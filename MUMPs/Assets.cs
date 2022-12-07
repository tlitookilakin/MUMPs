using AeroCore;
using AeroCore.Generics;
using AeroCore.Models;
using AeroCore.Utils;
using MUMPs.Integration;
using MUMPs.models;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;

namespace MUMPs
{
	[ModInit]
	internal static class Assets
	{
		const string ContentDir = "Mods/Mumps/";
		internal static Dictionary<string, AnimatedImage> Animations => animations.Value;
		internal static Dictionary<string, ParticleDefinition> Particles => particles.Value;
		internal static Dictionary<string, HorizonModel> Backdrops => backdrops.Value;
		internal static Dictionary<string, ForageData> Forage => forage.Value;
		internal static MiscGameData MiscGameData => miscGameData.Value;
		internal static GarbageData Garbage => garbage.Value;
		internal static Dictionary<string, SpawnData> Spawns => spawns.Value;

		private static readonly LazyAsset<Dictionary<string, AnimatedImage>> animations = 
			new(ModEntry.helper, static () => ContentDir + "Animations");
		private static readonly LazyAsset<Dictionary<string, ParticleDefinition>> particles =
			new(ModEntry.helper, static () => ContentDir + "Particles");
		private static readonly LazyAsset<Dictionary<string, HorizonModel>> backdrops =
			new(ModEntry.helper, static () => ContentDir + "Backgrounds");
		private static readonly LazyAsset<Dictionary<string, ForageData>> forage =
			new(ModEntry.helper, static () => ContentDir + "Forage");
		private static readonly LazyAsset<MiscGameData> miscGameData =
			new(ModEntry.helper, static () => "Data/MiscGameData");
		private static readonly LazyAsset<GarbageData> garbage =
			new(ModEntry.helper, static () => "Data/GarbageCans");
		private static readonly LazyAsset<Dictionary<string, SpawnData>> spawns =
			new(ModEntry.helper, static () => ContentDir + "MonsterSpawns");

		private static readonly string[] dirmap = {null, "up", "right", "down", "left"};

		internal static void Init()
		{
			ModEntry.helper.Events.Content.AssetRequested += LoadAssets;
		}
		private static void LoadAssets(object s, AssetRequestedEventArgs ev)
		{
			if (ev.Name.StartsWith(ContentDir))
			{
				string asset = ev.NameWithoutLocale.WithoutPath(ContentDir);
				string local = $"assets/{asset}.json";
				switch (asset)
				{
					case "Animations": ev.LoadFrom(() => ModEntry.helper.LoadLocalDict<AnimatedImage>(local), AssetLoadPriority.Low); break;
					case "Particles": ev.LoadFrom(() => ModEntry.helper.LoadLocalDict<ParticleDefinition>(local), AssetLoadPriority.Low); break;
					case "Backgrounds": ev.LoadFrom(() => ModEntry.helper.LoadLocalDict<HorizonModel>(local), AssetLoadPriority.Low); break;
					case "Forage": ev.LoadFrom(() => ModEntry.helper.LoadLocalDict<ForageData>(local), AssetLoadPriority.Low); break;
					case "MonsterSpawns": ev.LoadFrom(() => ModEntry.helper.LoadLocalDict<SpawnData>(local), AssetLoadPriority.Low); break;
				}
			}
			if (ev.NameWithoutLocale.IsEquivalentTo("Data/MiscGameData"))
				ev.LoadFrom(LoadMiscData, AssetLoadPriority.Low);
			else if (ev.NameWithoutLocale.IsEquivalentTo("data/GarbageData"))
				ev.LoadFromModFile<GarbageData>("assets/garbage.json", AssetLoadPriority.Low);
		}
		private static MiscGameData LoadMiscData()
		{
			var src = ModEntry.helper.ModContent.Load<MiscGameData>("assets/MiscGameData.json");
			if (!ModEntry.helper.ModRegistry.IsLoaded("mod.kitchen.minecartpatcher"))
				return src;
			var imports = ModEntry.helper.GameContent.Load<Dictionary<string, MinecartPatcherItem>>("MinecartPatcher.Minecarts");
			foreach((var key, var val) in imports)
			{
				if (key.StartsWith("minecartpatcher"))
					continue;
				src.MineCartDestinations.Add(key, new()
				{
					Location = val.LocationName,
					Tile = new(val.LandingPointX, val.LandingPointY),
					Direction = dirmap[Math.Clamp(val.LandingPointDirection + 1, 0, 4)],
					Network = val.NetworkId,
					DisplayName = val.DisplayName,
					Condition = val.MailCondition is null ? null : $"PLAYER_HAS_FLAG Current {val.MailCondition}"
				});
			}
			return src;
		}
	}
}
