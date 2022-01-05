using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MUMPs
{
    public class ModEntry : Mod
    {
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            ModEntry.monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            helper.Events.GameLoop.DayStarted += Events.DayStarted;
            helper.Events.Player.Warped += Events.ChangeLocation;
            helper.Events.Display.RenderedWorld += Events.DrawOnTop;
            helper.Events.GameLoop.UpdateTicked += Events.Tick;
            helper.Events.GameLoop.ReturnedToTitle += Events.OnQuit;
            helper.Events.GameLoop.SaveLoaded += Events.EnterWorld;
            Patch();
        }
        public static void Patch()
        {
            harmony.Patch(typeof(GameLocation).GetMethod("getFishingLocation"), new HarmonyMethod(typeof(Props.FishingArea).GetMethod("GetFishingLocationPatch")));
            harmony.Patch(typeof(GameLocation).GetMethod("DrawBackground"), new HarmonyMethod(typeof(Props.Horizon).GetMethod("DrawBackgroundPrefix")));
        }
    }
}
