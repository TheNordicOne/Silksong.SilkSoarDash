using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitTransitionGate : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  StickNeedle OFF
            // 2  play the impact clip
            //    - audio  hornet_superjump_pt_5_needle_impact_2d_distant
            // 3  PlayedThrowWait true -> finish here

            Finish();
        }

        public override void OnUpdate()
        {
            // 4  finish when the Super Jump Throw Wait animation completes
        }
    }
}
