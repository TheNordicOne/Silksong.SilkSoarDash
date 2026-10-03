using HarmonyLib;
using SilkSoarDash.Config;
using SilkSoarDash.Controls;
using SilkSoarDash.CustomFsm;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.Patches
{
    public static class SsdHeroPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanHarpoonDash))]
        // ReSharper disable once InconsistentNaming - Harmony Prefix Matching
        private static bool CanHarpoonDashPrefix(ref bool __result)
        {
            if (!SsdInput.DashDirectionPressed())
            {
                return true;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "IsPressingOnlyDown")]
        // ReSharper disable once InconsistentNaming - Harmony Prefix Matching
        private static bool IsPressingOnlyDownPrefix(ref bool __result)
        {
            // only the vanilla Silk Soar start reads this
            if (!SsdConfig.SwapDirections || SsdInput.Actions == null)
            {
                return true;
            }

            __result = SsdInput.PressingOnlyUp();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Extensions), nameof(global::Extensions.SendEventSafe), typeof(PlayMakerFSM), typeof(string))]
        private static bool SendEventSafePrefix(PlayMakerFSM fsm, string eventName)
        {
            if (eventName != SsdEvents.EnterSprinting || !SsdHeroState.ExitedDashing || fsm != HeroController.instance.sprintFSM)
            {
                return true;
            }

            SilkSoarDashFsm.EnterDashing();
            return false;
        }
    }
}
