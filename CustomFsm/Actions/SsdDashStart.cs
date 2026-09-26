using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStart : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private const float CancelableTime = 0.2f; 
        
        public override void OnEnter()
        {
            // 1  QueuedCancel
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;
            
            // 2  Set CancelableTime
            Fsm.GetFsmFloat(SsdVars.CancelableTime).Value = CancelableTime;
            
            // 3  HeroController.DoHardLandingEffectNoHit()
            Hero.DoHardLandingEffectNoHit();
            
            // 4  play the launch clip
            //    - audio      hornet_superjump_pt_7_hornet_jump_big_2d
            //    - vibration  super_jump_dash_burst
            
            // 5  Special Attacks/Super Jump Damager ON
            Hero.transform.Find("Special Attacks/Super Jump Damager").gameObject.SetActive(true);
            
            // 6  start the dash animation
            //    - anim  Super Jump Loop
            
            // 7  HeroController.SetCState("freezeCharge", false)
            Hero.SetCState("freezeCharge", false);
            
            // 8  HeroController.SetCState("superDashing", true)
            Hero.SetCState("superDashing", true);
            
            // 9  shake the camera
            //    - event  SuperDashShake
            
            // 10 HeroController.AffectedByGravity(false)
            Hero.AffectedByGravity(false);
            
            // 11 velocity = (JumpSpeed * Direction, 0)
            var jumpSpeed = Fsm.GetFsmFloat(SsdVars.JumpSpeed).Value;
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            Hero.Body.linearVelocity = new Vector2(jumpSpeed * direction, 0f);
            
            // 12 CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            // 13 send SUPER JUMP LAUNCH to the register
            EventRegister.SendEvent("SUPER JUMP LAUNCH");
            
            Finish();
        }
    }
}
