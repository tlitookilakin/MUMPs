﻿using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MUMPs
{
	public class ModEntry : Mod
	{
		public static readonly string ContentDir = PathUtilities.NormalizeAssetName("Mods/Mumps") + PathUtilities.PreferredAssetSeparator;

		internal static ITranslationHelper i18n;
		internal static IMonitor monitor;
		internal static IModHelper helper;
		internal static Harmony harmony;
		internal static string ModID;
		internal static AeroCore.API.API AeroAPI;

		internal static event Action<SpriteBatch> OnDraw;
		internal static event Action<GameLocation, bool> OnChangeLocation;
		internal static event Action OnCleanup;
		internal static event Action OnTick;

		internal static Dictionary<string, string> strings;
		public override void Entry(IModHelper helper)
		{
			monitor = Monitor;
			ModEntry.helper = Helper;
			harmony = new(ModManifest.UniqueID);
			i18n = helper.Translation;
			ModID = ModManifest.UniqueID;
			strings = helper.ModContent.Load<Dictionary<string, string>>("assets/strings.json");
			helper.Events.GameLoop.GameLaunched += Init;
			helper.Events.Content.AssetRequested += LoadAssets;
			helper.Events.Display.RenderedWorld += (s, a) => OnDraw?.Invoke(a.SpriteBatch);
			helper.Events.Player.Warped += (s, a) => OnChangeLocation?.Invoke(a.NewLocation, false);
			helper.Events.GameLoop.SaveLoaded += (s, a) => OnChangeLocation?.Invoke(Game1.currentLocation, false);
			helper.Events.GameLoop.ReturnedToTitle += (s, a) => OnCleanup?.Invoke();
			helper.Events.GameLoop.UpdateTicked += (s, a) => OnTick?.Invoke();
			helper.Events.Content.AssetsInvalidated += RefreshMap;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Init(object _, GameLaunchedEventArgs ev)
		{
			AeroAPI = AeroCore.ModEntry.GetStaticApi();
			AeroAPI.InitAll();
			harmony.PatchAll();
			monitor.Log(i18n.Get("startup"), LogLevel.Debug);
		}
		public static void LoadAssets(object _, AssetRequestedEventArgs ev)
		{
			if (ev.Name.IsEquivalentTo("Mods/Mumps/Fog"))
				ev.LoadFromModFile<Texture2D>("assets/fog.png", AssetLoadPriority.Medium);
		}
		private static void RefreshMap(object _, AssetsInvalidatedEventArgs ev)
		{
			if (Game1.currentLocation is null)
				return;
			var map = Game1.currentLocation.mapPath.Value;
			foreach(var name in ev.NamesWithoutLocale)
				if(name.IsEquivalentTo(map))
					OnChangeLocation?.Invoke(Game1.currentLocation, true);
		}
	}
}
