using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirCancel : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // effect  Effects/Super Jump Catch Effect
            // audio   Grunt Hornet Voice

            // RetractNeedle OFF
            // RetractNeedle localPosition = (0, NeedleStartHeight, 0)

            // CameraTarget.SetSuperJump is vertical only. Skipped

            // Hero.SetCState(SuperDashing, false)

            // event  EnemyKillShake

            // Hero.SetStartWithUpdraftExit()

            // anim  Super Jump Loop Cancel
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Loop Cancel animation completes
            Finish();
        }
    }
}
