using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.Controls;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharged : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCharged>();

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            SsdSilkReserve.Release(SsdHeroState.SilkCost);
            Hero.TakeSilk(SsdHeroState.SilkCost);

            // effect  Hornet_Super_Jump_Ready_Burst
            SsdEffects.StopChargeLoop();
            SsdEffects.PlayOneShot(SsdAudio.ChargeReady, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
            SsdEffects.PlayReady();
            SsdEffects.StartLoop(SsdAudio.CrazyCloakLoop);

            var chargedEffect = SsdClones.ChargedEffect;
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(true);
            }

            SsdFlash.StartSuperDash();
            Fsm.GetFsmBool(SsdVars.DidStartFlash).Value = true;

            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus).Value = false;
            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus2).Value = true;

            SsdShake.SetFocus(false);
            SsdShake.SetFocus2(true);
            SsdShake.Send(SsdCamera.AverageShake);
        }

        public override void OnExit()
        {
            SsdEffects.StopReady();
        }

        public override void OnUpdate()
        {
            var ia = SsdInput.Actions;
            if (ia == null)
            {
                return;
            }

            if (Hero.IsFalling())
            {
                SsdLog.Debug("throwing reason={Reason}", "falling");
                Fsm.Event(SsdEvents.ThrowNeedleStart);
                return;
            }

            if (ia.SuperDash.IsPressed)
            {
                return;
            }

            SsdLog.Debug("throwing reason={Reason}", "released");
            Fsm.Event(SsdEvents.ThrowNeedleStart);
        }
    }
}