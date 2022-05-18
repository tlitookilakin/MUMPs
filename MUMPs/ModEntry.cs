using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace MUMPs
{
    public class ModEntry : Mod, IAssetLoader
    {
        public static readonly string ContentDir = PathUtilities.NormalizeAssetName("Mods/Mumps") + PathUtilities.PreferredAssetSeparator;

        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;
        internal static API API = new();
        internal static AeroCore.API.API AeroAPI;

        public static Dictionary<string, string> strings;
        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            strings = helper.Content.Load<Dictionary<string, string>>("assets/strings.json");
            AeroAPI = (AeroCore.API.API)helper.ModRegistry.GetApi("tlitookilakin.AeroCore");

            harmony.PatchAll();
            AeroAPI.InitAll(typeof(ModEntry));
            RegisterActions();
        }
        public override object GetApi() => API;
        public static void RegisterActions()
        {
            AeroAPI.RegisterAction("Image",Props.ActionImage.show, 5);
            AeroAPI.RegisterAction("Repair", Props.ActionRepair.DoAction, 6);
            AeroAPI.RegisterAction("WarpList", Props.ActionWarpList.display);
            if (!helper.ModRegistry.IsLoaded("furyx639.GarbageDay"))
                AeroAPI.RegisterAction("Garbage", Props.ActionGarbage.HandleAction);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/EventVoid") ||
                   asset.AssetNameEquals("Mods/Mumps/Fog") ||
                   asset.AssetNameEquals("Mods/Mumps/Backgrounds/Empty") ||
                   asset.AssetNameEquals("Mods/Mumps/Backgrounds/Default");
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps/EventVoid"))
                return helper.Content.Load<T>("assets/eventvoid.tbin");
            else if (asset.AssetNameEquals("Mods/Mumps/Fog"))
                return helper.Content.Load<T>("assets/fog.png");
            else if (asset.AssetName.StartsWith(PathUtilities.NormalizeAssetName("Mods/Mumps/Backgrounds")))
                return helper.Content.Load<T>("assets/backgrounds/" + asset.AssetName.Split(PathUtilities.PreferredAssetSeparator).Last() + ".json");
            return (T)asset;
        }
    }
}
