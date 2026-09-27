using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancel : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCancel>();

        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool => SilkSpool.Instance;

        private static readonly string[] EffectPaths =
        {
            SsdObjects.ExtraGroundEffect,
            SsdObjects.ExtraThrowEffect,
            SsdObjects.Thread,
            SsdObjects.Damager
        };

        public override void OnEnter()
        {
            var previous = Fsm.PreviousActiveState;
            var transition = Fsm.LastTransition;

            SsdLog.Debug("cancelled from={State} event={Event}", previous == null ? "none" : previous.Name, transition == null ? "none" : transition.EventName);

            // the game broadcasts its cancel events to every FSM, so ignore them while the ability is not running
            if (previous == null || previous.Name == SsdStates.Inactive)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            if (Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value)
            {
                Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
            }

            foreach (var path in EffectPaths)
            {
                var effect = Hero.transform.Find(path);
                if (effect != null)
                {
                    effect.gameObject.SetActive(false);
                }
            }

            // audio  stop the charge loop
            // audio  stop Sounds/Superjump Loop

            Hero.SetCState(SsdCStates.SuperDashing, false);
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            // CameraTarget.SetSuperJump is vertical only. Skipped

            if (!Fsm.GetFsmBool(SsdVars.DidStartFlash).Value)
            {
                Finish();
                return;
            }

            // flash  cancel the sprite flash by its stored id

            Finish();
        }
    }
}
