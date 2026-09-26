using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWallHard : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // DeliveryQuestItem.TakeHit()
            // CreateNoiseV2 at Normal intensity
            // StickNeedle OFF
            // Damager OFF

            // effect  Roof Slam Effect R
            // audio   Grunt Hornet Voice
            // shake   Average Shake
            // anim    Super Jump Hit Roof

            // CameraTarget.SetSuperJump is vertical only. Skipped

            // EventRegister.SendEvent(SuperJumpEnded)

            // audio      hornet_land_hard new
            // vibration  hornet_land_hard

            // Hero.Body.linearVelocity = (0, 0)
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Hit Roof animation completes
            Finish();
        }
    }
}
