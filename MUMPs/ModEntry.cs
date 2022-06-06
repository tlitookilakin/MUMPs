using AeroCore.Utils;
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
using System.Linq;

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
        internal static API API = new();
        internal static AeroCore.API.API AeroAPI;

        internal static event Action<SpriteBatch> OnDraw;
        internal static event Action<GameLocation> OnChangeLocation;
        internal static event Action OnCleanup;
        internal static event Action OnTick;

        private static string[] LocalHorizons = {"Empty", "Default"};

        public static Dictionary<string, string> strings;
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
        }
        public override object GetApi() => API;
        private void Init(object _, GameLaunchedEventArgs ev)
        {
            AeroAPI = AeroCore.ModEntry.GetStaticApi();
            harmony.PatchAll();
            AeroAPI.InitAll();
            monitor.Log(i18n.Get("startup"), LogLevel.Debug);
        }
        public static void LoadAssets(object _, AssetRequestedEventArgs ev)
        {
            if (ev.Name.IsEquivalentTo("Maps/EventVoid"))
                ev.LoadFromModFile<xTile.Map>("assets/eventvoid.tbin", AssetLoadPriority.Medium);
            else if (ev.Name.IsEquivalentTo("Mods/Mumps/Fog"))
                ev.LoadFromModFile<Texture2D>("assets/fog.png", AssetLoadPriority.Medium);
            else if (ev.Name.IsDirectlyUnderPath("Mods/Mumps/Backgrounds"))
            {
                var n = ev.Name.WithoutPath("Mods/Mumps/Backgrounds");
                if (LocalHorizons.Contains(n))
                    ev.LoadFromModFile<HorizonModel>($"assets/backgrounds/{n}.json", AssetLoadPriority.Low);
            }
        }
    }
}
