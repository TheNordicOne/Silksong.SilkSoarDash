using System;
using BepInEx;
using HarmonyLib;

namespace SilkSoarDash
{
    [BepInPlugin("com.thenoridcone.silksoardash", "Silk Soar Dash", "1.0.0 ")]
    public class SilkSoardDash : BaseUnityPlugin
    {
        private static readonly BepInEx.Logging.ManualLogSource SSDLog = BepInEx.Logging.Logger.CreateLogSource("SilkSoarDash");

        private void Awake()
        {
            SSDLog.LogInfo("Plugin loaded and initialized.");

            Harmony.CreateAndPatchAll(typeof(SilkSoardDash), null);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanHarpoonDash))]
        private static bool CanHarpoonDashPrefix(ref bool __result)
        {
            if (!SilkSoarDashDirectionPressed())
            {
                return true;
            }

            SSDLog.LogInfo("SilkSoarDash Direction Pressed - Skipping Harpoon Dash");
            __result = false;
            return false;

        }

        private void Update()
        {
            if (PressedSilkSoarDash())
            {
                SSDLog.LogInfo("Dashing!");
            }
        }

        private bool PressedSilkSoarDash()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return false;
            }

            return ia.SuperDash.WasPressed && SilkSoarDashDirectionPressed();
        }

        private static bool SilkSoarDashDirectionPressed()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return false;
            }

            return ia.Up;
        }
    }
}