using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitSpikes : FsmStateAction
    {
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.DidHit).Value = false;
            Finish();
        }
    }
}