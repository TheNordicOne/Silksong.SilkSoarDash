using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWall : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // StickNeedle OFF
            // Damager OFF

            // CameraTarget.SetSuperJump is vertical only. Skipped

            // Hero.SetCState(SuperDashOnWall, true)
            // Hero.SetCState(SuperDashing, false)
            // Hero.AffectedByGravity(false)
            // SendMessage SetPlaySuperJumpFall(false) on the hero
            // Hero.Body.linearVelocity = (0, 0)
        }

        public override void OnUpdate()
        {
            // finish when the current animation completes
            Finish();
        }
    }
}
