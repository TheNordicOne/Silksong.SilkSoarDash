using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace SilkSoarDash.Config
{
    public static class SsdConfig
    {
        private const string GeneralSection = "General";

        // releases up to 1.1.0 saved the settings under a misspelled plugin id
        private const string OldConfigFile = "com.thenoridcone.silksoardash.cfg";

        // a fully upgraded spool holds 18, the game's own full spool achievement target
        private const int MaxSilkCost = 18;

        private static ConfigEntry<SsdAvailability> _availability;
        private static ConfigEntry<int> _silkCost;
        private static ConfigEntry<bool> _swapDirections;

        public static SsdAvailability Availability => _availability.Value;
        public static int SilkCost => _silkCost.Value;
        public static bool SwapDirections => _swapDirections.Value;

        public static void Bind(ConfigFile config)
        {
            MigrateOldFile(config);

            _availability = config.Bind(GeneralSection, "Availability", SsdAvailability.SilkSoar, "Which ability unlocks the Silk Soar Dash. Always makes it available from the start.");
            _swapDirections = config.Bind(GeneralSection, "Swap Directions", false, "Off: Up + Silk Soar button starts the Silk Soar Dash, Down starts the Silk Soar. On: the other way around.");
            _silkCost = config.Bind(GeneralSection, "Silk Cost", 1, new ConfigDescription("Silk used per Silk Soar Dash. 0 makes it free.", new AcceptableValueRange<int>(0, MaxSilkCost)));
        }

        private static void MigrateOldFile(ConfigFile config)
        {
            var oldPath = Path.Combine(Paths.ConfigPath, OldConfigFile);
            if (!File.Exists(oldPath) || File.Exists(config.ConfigFilePath))
            {
                return;
            }

            File.Move(oldPath, config.ConfigFilePath);
            config.Reload();
        }
    }
}
