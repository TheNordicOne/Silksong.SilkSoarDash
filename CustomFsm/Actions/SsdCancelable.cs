using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelable : FsmStateAction
    {
        public override void OnEnter()
        {
            SsdEffects.StartLoop(SsdAudio.FlyingLoop);
            SsdEffects.StartLoopVibration();

            var queuedCancel = Fsm.GetFsmBool(SsdVars.QueuedCancel).Value;
            if (queuedCancel)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdEffects.StartRumble();
        }

        public override void OnExit()
        {
            SsdEffects.StopLoop();
            SsdEffects.StopRumble();
        }
    }
}
