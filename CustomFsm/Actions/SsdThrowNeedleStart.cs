using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedleStart : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            Activate(SsdClones.ExtraThrowEffect);
            Activate(SsdClones.HarpoonThrowEffect);

            var fader = SsdClones.ChargingFader;
            if (fader != null)
            {
                fader.FadeTo(0f, SsdVars.ChargingFaderFadeTime);
            }

            EndAnticEffect(SsdClones.AnticEffectL);
            EndAnticEffect(SsdClones.AnticEffectR);

            SsdFlash.Cancel();
            Fsm.GetFsmBool(SsdVars.DidStartFlash).Value = false;

            SsdShake.SetFocus(false);
            SsdShake.SetFocus2(false);
            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus2).Value = false;

            // message  SendMessageV2 to the hero

            Hero.PlayAnim(SsdAnims.Throw);

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpThrowNeedle);
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.Throw))
            {
                return;
            }

            Finish();
        }

        private static void Activate(Transform effect)
        {
            if (effect != null)
            {
                effect.gameObject.SetActive(true);
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
