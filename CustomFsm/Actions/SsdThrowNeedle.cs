using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedle : FsmStateAction
    {
        public override void OnEnter()
        {
            Fsm.Event(SsdEvents.Cancelled);
            Finish();
        }
    }
}