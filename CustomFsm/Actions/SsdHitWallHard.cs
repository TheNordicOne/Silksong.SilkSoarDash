using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
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
            var damager = Hero.transform.Find(SsdObjects.Damager);
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);

            // effect  Roof Slam Effect R
            // audio   Grunt Hornet Voice
            // shake   Average Shake
            // anim    Super Jump Hit Roof

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);

            // audio      hornet_land_hard new
            // vibration  hornet_land_hard

            Hero.Body.linearVelocity = new Vector2(0, 0);
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Hit Roof animation completes
            Finish();
        }
    }
}
