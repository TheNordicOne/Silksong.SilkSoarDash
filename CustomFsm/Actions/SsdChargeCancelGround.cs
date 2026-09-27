using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdChargeCancelGround : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool => SilkSpool.Instance;

        public override void OnEnter()
        {
            if (Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value)
            {
                Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
            }

            Deactivate(SsdClones.ExtraGroundEffect);

            // audio   stop the charge loop

            Deactivate(SsdClones.AnticEffectL);
            Deactivate(SsdClones.AnticEffectR);

            // vanilla fades the fader out over 0.1, this snaps it
            ClearChargingFader();

            Hero.SetCState(SsdCStates.FreezeCharge, false);

            Hero.PlayAnim(SsdAnims.AnticCancel);
            EndAnticEffect(SsdClones.AnticEffectL);
            EndAnticEffect(SsdClones.AnticEffectR);

            SsdShake.SetFocus(false);
            SsdShake.SetFocus2(false);

            Hero.Body.linearVelocity = Vector2.zero;
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.AnticCancel))
            {
                return;
            }

            Finish();
        }

        private static void Deactivate(Transform effect)
        {
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
            }
        }

        private static void EndAnticEffect(Transform anticEffect)
        {
            if (anticEffect != null)
            {
                anticEffect.PlayAnim(SsdAnims.AnticEffectEnd);
            }
        }

        private static void ClearChargingFader()
        {
            var fader = SsdClones.ChargingFader;
            if (fader == null)
            {
                return;
            }

            var group = fader.GetComponent<NestedFadeGroupBase>();
            if (group != null)
            {
                group.AlphaSelf = 0f;
            }
        }
    }
}
