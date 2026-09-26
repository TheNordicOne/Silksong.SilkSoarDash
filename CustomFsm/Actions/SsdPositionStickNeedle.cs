using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedle : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  read HeroX
            // 2  isDistant = StickNeedleX > HeroX + 15
            // 3  clip = distant clip if isDistant, near clip otherwise
            //    - audio  hornet_superjump_pt_5_needle_impact_2d_distant
            //    - audio  hornet_superjump_pt_5_needle_impact_2d
            // 4  play the clip
            // 5  PlayedThrowWait true -> finish here
            // 6  finish when the Super Jump Throw Wait animation completes

            Finish();
        }
    }
}
