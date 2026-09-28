using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStartQuick : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            SsdClones.Damager.gameObject.SetActive(true);
            SsdClones.DashEffect.gameObject.SetActive(true);
            
            Hero.PlayAnim(SsdAnims.Loop);
            
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            SsdHeroState.Dashing = true;
            
            Hero.RelinquishControlNotVelocity();
            
            Hero.AffectedByGravity(false);

            Hero.ApplySsdVelocity(Fsm);

            
            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            Finish();
        }
    }
}
