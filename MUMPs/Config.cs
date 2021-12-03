using StardewModdingAPI;

namespace MUMPs
{
    class Config
    {
        public SButton debugKey { get; set; }

        public Config()
        {
            debugKey = SButton.J;
        }
    }
}
