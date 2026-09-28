using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStart : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;
            
            Fsm.GetFsmFloat(SsdVars.CancelableTime).Value = SsdVars.DefaultCancelableTime;
            
            Hero.DoHardLandingEffectNoHit();

            SsdEffects.PlayOneShot2D(SsdAudio.JumpBig, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
            SsdEffects.Vibrate(SsdVibration.DashBurst);
            
            SsdClones.Damager.gameObject.SetActive(true);
            SsdClones.DashEffect.gameObject.SetActive(true);
            
            Hero.EnterDashPose(Fsm.GetFsmFloat(SsdVars.Direction).Value);
            
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            SsdHeroState.Dashing = true;
            
            Hero.RelinquishControlNotVelocity();
            
            SsdShake.Send(SsdCamera.SuperDashShake);
            
            Hero.AffectedByGravity(false);

            Hero.ApplySsdVelocity(Fsm);

            
            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpLaunch);
            
            Finish();
        }
    }
}
