using AeroCore.Generics;
using AeroCore.Models;
using MUMPs.models;
using System;
using System.Collections.Generic;

namespace MUMPs
{
    internal class Assets
    {
        const string ContentDir = "Mods/Mumps/";
        internal static Dictionary<string, AnimatedImage> Animations => animations.Value;
        internal static Dictionary<string, ParticleDefinition> Particles => particles.Value;
        internal static Dictionary<string, HorizonModel> Backdrops => backdrops.Value;
        internal static Dictionary<string, DestinationList> Destinations => warpLists.Value;
        internal static Dictionary<string, ForageData> Forage => forage.Value;

        private static readonly LazyAsset<Dictionary<string, AnimatedImage>> animations = 
            new(ModEntry.helper, static () => ContentDir + "Animations");
        private static readonly LazyAsset<Dictionary<string, ParticleDefinition>> particles =
            new(ModEntry.helper, static () => ContentDir + "Particles");
        private static readonly LazyAsset<Dictionary<string, HorizonModel>> backdrops =
            new(ModEntry.helper, static () => ContentDir + "Backgrounds");
        private static readonly LazyAsset<Dictionary<string, DestinationList>> warpLists =
            new(ModEntry.helper, static () => ContentDir + "CustomDestinations");
        private static readonly LazyAsset<Dictionary<string, ForageData>> forage =
            new(ModEntry.helper, static () => ContentDir + "Forage");
    }
}
