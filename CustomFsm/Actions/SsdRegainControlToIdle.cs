using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRegainControlToIdle : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            Hero.SetCState(SsdCStates.SuperDashOnWall, false);
            Hero.SetCState(SsdCStates.SuperDashing, false);
            
            Hero.RegainControl();
            Hero.StartAnimationControlToIdle();
            Hero.AffectedByGravity(true);

            Finish();
        }
    }
}