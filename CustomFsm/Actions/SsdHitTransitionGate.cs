using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitTransitionGate : FsmStateAction
    {
        
        public override void OnEnter()
        {
            var needleStick = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            needleStick.SetActive(false);
            
            // audio  hornet_superjump_pt_5_needle_impact_2d_distant
            // PlayedThrowWait true -> finish here
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Throw Wait animation completes
            Finish();
        }
    }
}
