using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancel : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCancel>();

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var previous = Fsm.PreviousActiveState;
            var transition = Fsm.LastTransition;

            // the game broadcasts its cancel events to every FSM, so ignore them while the ability is not running
            if (previous == null || previous.Name == SsdStates.Inactive)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdLog.Debug("cancelled from={State} event={Event}", previous.Name, transition == null ? "none" : transition.EventName);

            if (Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value)
            {
                SsdSilkReserve.Release(SsdHeroState.SilkCost);
            }

            Deactivate(SsdClones.ExtraGroundEffect);
            Deactivate(SsdClones.ExtraThrowEffect);
            Deactivate(SsdClones.Thread);
            Deactivate(SsdClones.Damager);

            SsdEffects.StopChargeLoop();
            SsdEffects.StopLoop();

            SsdHeroState.Dashing = false;
            if (!(previous.Name == SsdStates.LeavingScene && SsdHeroState.ExitedDashing))
            {
                Hero.ExitDashPose();
            }
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            // CameraTarget.SetSuperJump is vertical only. Skipped

            if (!Fsm.GetFsmBool(SsdVars.DidStartFlash).Value)
            {
                Finish();
                return;
            }

            SsdFlash.Cancel();

            Finish();
        }

        private static void Deactivate(Transform effect)
        {
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
            }
        }
    }
}
