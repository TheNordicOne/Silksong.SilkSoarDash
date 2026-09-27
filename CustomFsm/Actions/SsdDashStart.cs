using BepInEx.Logging;
using GlobalEnums;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStart : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdDashStart>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;
            
            Fsm.GetFsmFloat(SsdVars.CancelableTime).Value = SsdVars.DefaultCancelableTime;
            
            Hero.DoHardLandingEffectNoHit();

            // audio      hornet_superjump_pt_7_hornet_jump_big_2d
            // vibration  super_jump_dash_burst
            
            SsdClones.Damager.gameObject.SetActive(true);
            
            Hero.PlayAnim(SsdAnims.Loop);
            
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            Hero.SetCState(SsdCStates.SuperDashing, true);
            
            Hero.RelinquishControlNotVelocity();
            
            SsdShake.Send(SsdCamera.SuperDashShake);
            
            Hero.AffectedByGravity(false);

            Hero.ApplySsdVelocity(Fsm);

            SsdLog.Debug("dashing direction={Direction} facingRight={FacingRight} scaleX={ScaleX} velocity={Velocity}", Fsm.GetFsmFloat(SsdVars.Direction).Value, Hero.cState.facingRight, Hero.transform.localScale.x, Hero.Body.linearVelocity);
            
            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpLaunch);
            
            Finish();
        }
    }
}
