using BepInEx;
using HarmonyLib;

namespace SilkSoarDash
{
    [BepInPlugin("com.thenoridcone.silksoardash", "Silk Soar Dash", "1.0.0 ")]
    public class SilkSoardDash : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Plugin loaded and initialized.");

            Harmony.CreateAndPatchAll(typeof(SilkSoardDash), null);
        }
    }
}