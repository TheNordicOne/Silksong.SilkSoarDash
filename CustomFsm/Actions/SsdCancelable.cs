using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelable : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // audio      Sounds/Superjump Loop
            // vibration  Sounds/Superjump Loop

            // QueuedCancel true -> cancel.
            var queuedCancel = Fsm.GetFsmBool(SsdVars.QueuedCancel).Value;
            if (queuedCancel)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            // shake  Tiny Rumble
        }
    }
}
