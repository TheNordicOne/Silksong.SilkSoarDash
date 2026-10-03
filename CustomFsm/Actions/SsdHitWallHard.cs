using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWallHard : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            DeliveryQuestItem.TakeHit();

            // noise  Normal

            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var damager = SsdClones.Damager;
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);

            // effect  Roof Slam Effect R
            SsdEffects.PlayVoice(SsdAudio.GruntVoice);
            SsdShake.Send(SsdCamera.AverageShake);

            Hero.ExitDashPoseKeepingFront(Fsm.GetFsmFloat(SsdVars.Direction).Value);
            Hero.PlayAnim(SsdAnims.Catch);

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);

            SsdEffects.PlayOneShot(SsdAudio.LandHard, SsdAudio.FlatPitch, SsdAudio.FlatPitch);
            SsdEffects.Vibrate(SsdVibration.LandHard);

            Hero.Body.linearVelocity =  Vector2.zero;
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.Catch))
            {
                return;
            }

            Finish();
        }
    }
}
