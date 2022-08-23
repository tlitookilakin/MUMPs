using AeroCore;
using AeroCore.Generics;
using AeroCore.Models;
using AeroCore.Utils;
using MUMPs.models;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MUMPs
{
    [ModInit]
    internal class Assets
    {
        const string ContentDir = "Mods/Mumps/";
        internal static Dictionary<string, AnimatedImage> Animations => animations.Value;
        internal static Dictionary<string, ParticleDefinition> Particles => particles.Value;
        internal static Dictionary<string, HorizonModel> Backdrops => backdrops.Value;
        internal static Dictionary<string, ForageData> Forage => forage.Value;

        private static readonly LazyAsset<Dictionary<string, AnimatedImage>> animations = 
            new(ModEntry.helper, static () => ContentDir + "Animations");
        private static readonly LazyAsset<Dictionary<string, ParticleDefinition>> particles =
            new(ModEntry.helper, static () => ContentDir + "Particles");
        private static readonly LazyAsset<Dictionary<string, HorizonModel>> backdrops =
            new(ModEntry.helper, static () => ContentDir + "Backgrounds");
        private static readonly LazyAsset<Dictionary<string, ForageData>> forage =
            new(ModEntry.helper, static () => ContentDir + "Forage");

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
                    case "Animations": ev.LoadFromModFile<Dictionary<string, AnimatedImage>>(local, AssetLoadPriority.Low); break;
                    case "Particles": ev.LoadFromModFile<Dictionary<string, ParticleDefinition>>(local, AssetLoadPriority.Low); break;
                    case "Backgrounds": ev.LoadFromModFile<Dictionary<string, HorizonModel>>(local, AssetLoadPriority.Low); break;
                    case "Forage": ev.LoadFromModFile<Dictionary<string, ForageData>>(local, AssetLoadPriority.Low); break;
                }
            }
        }
    }
}
