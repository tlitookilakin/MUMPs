using StardewModdingAPI;
using StardewValley;

namespace MUMPs
{
    public class ModEntry : Mod
    {
        internal ITranslationHelper i18n => Helper.Translation;

        public override void Entry(IModHelper helper)
        {
            string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            helper.Events.GameLoop.DayStarted += Events.DayStarted;
        }
    }
}
