using System;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm
{

    public static class FsmBuilder
    {
        public static FsmState State(Fsm fsm, string name, [CanBeNull] FsmTransition[] transitions)
        {
            return new FsmState(fsm)
            {
                Name = name,
                Actions = Array.Empty<FsmStateAction>(),
                Transitions = transitions ?? Array.Empty<FsmTransition>()
            };
        }
        public static FsmState State(Fsm fsm, string name, FsmStateAction[] actions,  [CanBeNull] FsmTransition[] transitions)
        {
            var state = new FsmState(fsm)
            {
                Name = name,
                Actions = actions,
                Transitions = transitions ?? Array.Empty<FsmTransition>(),
            };

            state.SaveActions();
            return state;
        }

        public static FsmTransition Transition(string eventName, string toState)
        {
            return new FsmTransition { FsmEvent = new FsmEvent(eventName), ToState = toState };
        }
        
        public static FsmTransition TransitionToInactive()
        {
            return new FsmTransition { FsmEvent = FsmEvent.Finished, ToState = SsdStates.Inactive };
        }
    }
}