using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCharge>();

        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool =>  SilkSpool.Instance;

        
        public override void OnEnter()
        {
            Hero.StopAnimationControl();
         
           var didAddUsingSilk =  Spool.AddUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
           Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value = didAddUsingSilk;

           var groundEffect = SsdClones.ExtraGroundEffect;
           if (groundEffect != null)
           {
               groundEffect.gameObject.SetActive(true);
           }

           Hero.GetComponent<tk2dSpriteAnimator>().Play(SsdAnims.Antic);

           SsdEffects.PlayOneShot(SsdAudio.IntoPosition, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
           SsdEffects.StartChargeLoop();

           PlayAnticEffect(SsdClones.AnticEffectL);
           PlayAnticEffect(SsdClones.AnticEffectR);
           Hero.SetCState(SsdCStates.FreezeCharge, true);

           StartRumblingFocus();

           StartChargingFader();
        }

        private static void PlayAnticEffect(Transform anticEffect)
        {
            if (anticEffect == null)
            {
                return;
            }

            anticEffect.gameObject.SetActive(true);
            anticEffect.PlayAnim(SsdAnims.AnticEffect);
        }

        private void StartRumblingFocus()
        {
            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus).Value = true;

            SsdShake.SetFocus(true);
            SsdShake.Send(SsdCamera.FocusRumble);
        }

        private void StartChargingFader()
        {
            var fader = SsdClones.ChargingFader;
            if (fader == null)
            {
                return;
            }

            fader.gameObject.SetActive(true);
            fader.SetAlpha(0f);
            fader.FadeTo(1f, Fsm.GetFsmFloat(SsdVars.ChargeTime).Value);
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            var released = !ia.SuperDash.IsPressed;
            var falling = Hero.IsFalling();

            if (!released && !falling)
            {
                return;
            }

            SsdLog.Debug("cancelled released={Released} falling={Falling}", released, falling);
            Fsm.Event(SsdEvents.Cancelled);
        }
    }
}
