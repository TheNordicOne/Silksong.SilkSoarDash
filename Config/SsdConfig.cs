using BepInEx.Configuration;

namespace SilkSoarDash.Config
{
    public static class SsdConfig
    {
        private const string GeneralSection = "General";

        // a fully upgraded spool holds 18, the game's own full spool achievement target
        private const int MaxSilkCost = 18;

        private static ConfigEntry<SsdAvailability> _availability;
        private static ConfigEntry<int> _silkCost;

        public static SsdAvailability Availability => _availability.Value;
        public static int SilkCost => _silkCost.Value;

        public static void Bind(ConfigFile config)
        {
            _availability = config.Bind(GeneralSection, "Availability", SsdAvailability.SilkSoar, "Which ability unlocks the Silk Soar Dash. Always makes it available from the start.");
            _silkCost = config.Bind(GeneralSection, "Silk Cost", 1, new ConfigDescription("Silk used per Silk Soar Dash. 0 makes it free.", new AcceptableValueRange<int>(0, MaxSilkCost)));
        }
    }
}
