using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
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

            var fader = SsdClones.ChargingFader;
            if (fader != null)
            {
                fader.FadeTo(0f, SsdVars.ChargingFaderFadeTime);
            }

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

    }
}
