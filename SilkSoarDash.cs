using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilkSoarDash.Config;
using SilkSoarDash.Extensions;
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

            SsdConfig.Bind(Config);

            Harmony.CreateAndPatchAll(typeof(SilkSoarDash));
            Harmony.CreateAndPatchAll(typeof(SsdSilkReserve));
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
            
            // a sprint passes CanSuperJump as a cancelable FSM move, so the game's own start cancels it and takes control back first
            if (Hero.controlReqlinquished)
            {
                EventRegister.SendEvent(EventRegisterEvents.FsmCancel);
                Hero.RegainControl();
                Hero.StartAnimationControlToIdle();
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

            return InputActions.SuperDash.WasPressed && SilkSoarDashDirectionPressed() && CanSilkSoarDash();
        }

        private static bool CanSilkSoarDash()
        {
            return HasUnlockingAbility(Hero.playerData) && Hero.CanStartSoar();
        }

        private static bool HasUnlockingAbility(PlayerData playerData)
        {
            switch (SsdConfig.Availability)
            {
                case SsdAvailability.Clawline:
                    return playerData.hasHarpoonDash;
                case SsdAvailability.Always:
                    return true;
                default:
                    return playerData.hasSuperJump;
            }
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