using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdQueueCancel : FsmStateAction
    {
        public override void OnEnter()
        {
            Fsm.GetFsmFloat(SsdVars.CancelableTime).Value = SsdVars.QueuedCancelableTime;
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = true;

            Finish();
        }
    }
}
