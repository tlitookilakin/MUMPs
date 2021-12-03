﻿using StardewModdingAPI;
using StardewValley;

namespace MUMPs
{
    public class ModEntry : Mod
    {
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            monitor = Monitor;
            helper.Events.GameLoop.DayStarted += Events.DayStarted;
            helper.Events.Player.Warped += Props.MoveWarps.CorrectWarp;
        }
    }
}
