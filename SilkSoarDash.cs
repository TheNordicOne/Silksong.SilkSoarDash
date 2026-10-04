using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilkSoarDash.Config;
using SilkSoarDash.Controls;
using SilkSoarDash.CustomFsm;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using SilkSoarDash.Patches;

namespace SilkSoarDash;

[BepInPlugin("com.thenordicone.silksoardash", "Silk Soar Dash", "1.1.1")]
public class SilkSoarDash : BaseUnityPlugin
{
    private static readonly ManualLogSource Log = SilkLog.For<SilkSoarDash>();

    private static HeroController Hero => HeroController.instance;

    private void Awake()
    {
        Log.Info("loaded");

        SsdConfig.Bind(Config);

        Harmony.CreateAndPatchAll(typeof(SsdHeroPatches));
        Harmony.CreateAndPatchAll(typeof(SsdSilkReserve));
    }

    private void Update()
    {
        if (SsdHeroState.CrossingRoom && SsdInput.PressedCancel())
        {
            SsdHeroState.CancelQueued = true;
            return;
        }

        if (!SsdInput.PressedSilkSoarDash() || !CanSilkSoarDash())
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
            case SsdAvailability.SilkSoar:
            default:
                return playerData.hasSuperJump;
        }
    }
}