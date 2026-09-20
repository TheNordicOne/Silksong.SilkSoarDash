using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitSpikes : FsmStateAction
    {
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.DidHit).Value = false;

            // Presentation
            // - shake  Small Shake
            // - audio  tink_effect

            Finish();
        }
    }
}