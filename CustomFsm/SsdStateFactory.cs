using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Actions;

namespace SilkSoarDash.CustomFsm
{
    public static class SsdStateFactory
    {
        public static FsmState Inactive(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Inactive,
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Start, SsdStates.Charge)
                });
        }

        public static FsmState Charge(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Charge,
                new FsmStateAction[] { new SsdCharge() },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Charged, SsdStates.Charged),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.Cancelled)
                });
        }

        public static FsmState Charged(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Charged,
                new FsmStateAction[] { new SsdCharged() },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.GetDistance, SsdStates.GetDistance)
                });
        }

        public static FsmState Cancelled(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Cancelled,
                new[]
                {
                    FsmBuilder.TransitionToInactive()
                });
        }

        public static FsmState GetDistance(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.GetDistance,
                new FsmStateAction[] { new SsdGetDistance() },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.ThrowNeedle, SsdStates.ThrowNeedle)
                });
        }
        
        public static FsmState ThrowNeedle(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.ThrowNeedle,
                new FsmStateAction[] {  },
                new[]
                {
                    FsmBuilder.TransitionToInactive()
                });
        }
    }
}
