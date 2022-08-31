using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;

namespace MUMPs.Integration
{
    internal class MinecartPatcherItem
    {
        internal string LocationName { get; set; }
        internal string DisplayName { get; set; }
        internal int LandingPointX { get; set; }
        internal int LandingPointY { get; set; }
        internal int LandingPointDirection { get; set; } = -1;
        internal bool IsUnderground { get; set; }
        internal string MailCondition { get; set; }
        internal string VanillaPassthrough { get; set; }
        internal string NetworkId { get; set; }
    }

    [ModInit(WhenHasMod = "mod.kitchen.minecartpatcher")]
    internal class MinecartPatcherPatch
    {
        internal static void Init()
        {
            ModEntry.monitor.Log("Attempting to patch MinecartPatcher...", LogLevel.Debug);
            var mcphook = AccessTools.DeclaredConstructor(AccessTools.TypeByName("MinecartPatcher.MCPModHooks"));
            if (mcphook is not null)
                ModEntry.harmony.Patch(mcphook, postfix: new(typeof(MinecartPatcherPatch).MethodNamed(nameof(ResetField))));
            else
                ModEntry.monitor.Log("Failed to find patch target. MinecartPatcher not patched.", LogLevel.Warn);
        }

        private static void ResetField(ModHooks __alias)
            => typeof(Game1).GetField("hooks", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, __alias);
    }
}
