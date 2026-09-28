using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.CustomFsm;
using SilkSoarDash.Logging;

namespace SilkSoarDash
{
    [BepInPlugin("com.thenoridcone.silksoardash", "Silk Soar Dash", "1.0.0 ")]
    public class SilkSoarDash : BaseUnityPlugin
    {
        private static readonly ManualLogSource Log = SilkLog.For<SilkSoarDash>();

        private static HeroActions InputActions => GameManager.instance?.inputHandler?.inputActions;
        private static HeroController Hero => HeroController.instance;

        private void Awake()
        {
            Log.Info("loaded");

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Extensions), nameof(global::Extensions.SendEventSafe), typeof(PlayMakerFSM), typeof(string))]
        private static bool SendEventSafePrefix(PlayMakerFSM fsm, string eventName)
        {
            if (eventName != SsdEvents.EnterSprinting || !SsdHeroState.ExitedDashing || fsm != Hero.sprintFSM)
            {
                return true;
            }

            SilkSoarDashFsm.EnterDashing();
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

            return InputActions.SuperDash.WasPressed && SilkSoarDashDirectionPressed() && Hero.CanSuperJump();
        }

        private static bool SilkSoarDashDirectionPressed()
        {
            if (InputActions == null)
            {
                return false;
            }

            return InputActions.Up.IsPressed;
        }
    }
}