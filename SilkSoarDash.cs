using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilkSoarDash.CustomFsm;

namespace SilkSoarDash
{
    [BepInPlugin("com.thenoridcone.silksoardash", "Silk Soar Dash", "1.0.0 ")]
    public class SilkSoarDash : BaseUnityPlugin
    {
        private static readonly ManualLogSource Log = SilkLog.For<SilkSoarDash>();

        private static HeroActions InputActions => GameManager.instance?.inputHandler?.inputActions;

        private void Awake()
        {
            Log.LogInfo("Plugin loaded and initialized.");

            Harmony.CreateAndPatchAll(typeof(SilkSoarDash));
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanHarpoonDash))]
        // ReSharper disable once InconsistentNaming - Harmony Prefix Matching
        private static bool CanHarpoonDashPrefix(ref bool __result)
        {
            if (!SilkSoarDashDirectionPressed())
            {
                return true;
            }

            __result = false;
            return false;
        }

        private void Update()
        {
            if (!PressedSilkSoarDash())
            {
                return;
            }
            
            SilkSoarDashFsm.Build();
            SilkSoarDashFsm.Trigger();
        }

        private static bool PressedSilkSoarDash()
        {
            if (InputActions == null)
            {
                return false;
            }

            return InputActions.SuperDash.WasPressed && SilkSoarDashDirectionPressed();
        }

        private static bool SilkSoarDashDirectionPressed()
        {
            if (InputActions == null)
            {
                return false;
            }

            return InputActions.Up;
        }
    }
}