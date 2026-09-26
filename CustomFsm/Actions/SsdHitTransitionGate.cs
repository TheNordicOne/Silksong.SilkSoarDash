using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitTransitionGate : FsmStateAction
    {
        
        public override void OnEnter()
        {
            Finish();
        }
    }
}