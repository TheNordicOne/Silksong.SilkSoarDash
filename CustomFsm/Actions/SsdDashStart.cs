using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStart : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  queuedCancel = false
            // 2  cancelableTime = 0.2
            // 3  HeroController.DoHardLandingEffectNoHit()
            // 4  play the launch clip
            //    - audio      hornet_superjump_pt_7_hornet_jump_big_2d
            //    - vibration  super_jump_dash_burst
            // 5  Super Jump Damager ON
            // 6  play the dash animation
            //    - anim  Super Jump Loop
            // 7  HeroController.SetCState("freezeCharge", false)
            // 8  HeroController.SetCState("superDashing", true)
            // 9  shake the camera
            //    - event  SuperDashShake
            // 10 HeroController.AffectedByGravity(false)
            // 11 velocity = 33 along Direction
            // 12 CameraTarget.SetSuperJump(true)
            // 13 send SUPER JUMP LAUNCH to the register
            // 14 finish

            Finish();
        }
    }
}
