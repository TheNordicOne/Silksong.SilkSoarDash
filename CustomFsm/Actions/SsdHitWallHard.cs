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
            
            // CreateNoiseV2 at Normal intensity
            
            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var damager = SsdClones.Damager;
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);

            // effect  Roof Slam Effect R
            // audio   Grunt Hornet Voice
            // shake   Average Shake

            Hero.PlayAnim(SsdAnims.WallCatch);

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);

            // audio      hornet_land_hard new
            // vibration  hornet_land_hard

            Hero.Body.linearVelocity =  Vector2.zero;
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.WallCatch))
            {
                return;
            }

            Finish();
        }
    }
}
