using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRetractNeedleCancel : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // QueuedCancel = false

            // audio  hornet_superjump_cancel

            // read RetractNeedle X
            // read RetractNeedle Move To child X
            // needleDistance = Move To X - RetractNeedle X
            // RetractNeedle ON
            // StickNeedle OFF
            // moveBy = (needleDistance, 0, 0)
            // Damager OFF
            // tween RetractNeedle by moveBy at speed 150, linear, world space
            // DecelerateV2 on the hero

            // CameraTarget.SetSuperJump is vertical only. Skipped

            // EventRegister.SendEvent(SuperJumpEnded)
        }

        public override void OnUpdate()
        {
            // finish when RetractNeedle reaches the target
            Finish();
        }
    }
}
