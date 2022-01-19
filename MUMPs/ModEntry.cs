using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

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

        public static Dictionary<string, string> strings;
        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            strings = helper.Content.Load<Dictionary<string, string>>("assets/strings.json");
            helper.Events.GameLoop.DayStarted += Events.DayStarted;
            helper.Events.Player.Warped += Events.ChangeLocation;
            helper.Events.Display.RenderedWorld += Events.DrawOnTop;
            helper.Events.GameLoop.UpdateTicked += Events.Tick;
            helper.Events.GameLoop.ReturnedToTitle += Events.OnQuit;
            helper.Events.GameLoop.SaveLoaded += Events.EnterWorld;
            helper.Events.Display.RenderedHud += Events.DrawOverHud;
            helper.Events.Multiplayer.ModMessageReceived += Events.RecieveMessage;
            Patches.Lighting.OnLighting += Events.DoLighting;
            harmony.PatchAll();
            RegisterActions();
        }
        public static void RegisterActions()
        {
            Utils.AddAction("Image", true, Props.ActionImage.show);
            Utils.AddAction("Repair", false, Props.ActionRepair.DoAction);
            Utils.AddAction("WarpList", false, Props.ActionWarpList.display);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/EventVoid");
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps/EventVoid"))
                return helper.Content.Load<T>("assets/eventvoid.tbin");
            return (T)asset;
        }
    }
}
