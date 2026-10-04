using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdQueueCancel : FsmStateAction
    {
        public override void OnEnter()
        {
            Queue(Fsm);

            Finish();
        }

        public static void Queue(Fsm fsm)
        {
            fsm.GetFsmFloat(SsdVars.CancelableTime).Value = SsdVars.QueuedCancelableTime;
            fsm.GetFsmBool(SsdVars.QueuedCancel).Value = true;
        }
    }
}
