using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitSpikes : FsmStateAction
    {
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.DidHit).Value = false;

            // - shake  Small Shake
            // - audio  tink_effect

            Finish();
        }
    }
}