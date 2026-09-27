using GlobalEnums;
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

            // audio      hornet_superjump_pt_7_hornet_jump_big_2d
            // vibration  super_jump_dash_burst
            
            SsdClones.Damager.gameObject.SetActive(true);
            
            // anim  Super Jump Loop
            
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            Hero.SetCState(SsdCStates.SuperDashing, true);
            
            Hero.hero_state = ActorStates.no_input;
            
            // event  SuperDashShake
            
            Hero.AffectedByGravity(false);

            Hero.ApplySsdVelocity(Fsm);
            
            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpLaunch);
            
            Finish();
        }
    }
}
