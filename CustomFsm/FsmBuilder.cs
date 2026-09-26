using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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

        public static FsmState State(Fsm fsm, string name, FsmStateAction[] actions, [CanBeNull] FsmTransition[] transitions)
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
            return new FsmTransition
            {
                FsmEvent = new FsmEvent(eventName),
                ToState = toState
            };
        }

        public static Wait WaitFor(Fsm fsm, string timeVariable, string finishEvent)
        {
            return new Wait
            {
                time = fsm.GetFsmFloat(timeVariable),
                finishEvent = new FsmEvent(finishEvent),
                realTime = false
            };
        }

        public static Wait WaitFor(float time, string finishEvent)
        {
            return new Wait
            {
                time = new FsmFloat
                {
                    Value = time
                },
                finishEvent = new FsmEvent(finishEvent),
                realTime = false
            };
        }

        public static ActivateGameObjectDelay ActivateAfter(string heroPath, float delay)
        {
            var target = HeroController.instance.transform.Find(heroPath);

            return new ActivateGameObjectDelay
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = new FsmGameObject
                    {
                        Value = target == null ? null : target.gameObject
                    }
                },
                activate = new FsmBool
                {
                    Value = true
                },
                resetOnExit = true,
                delay = new FsmFloat
                {
                    Value = delay
                }
            };
        }

        public static CheckCollisionSide CheckSides(Fsm fsm, string leftVariable, string rightVariable)
        {
            return new CheckCollisionSide
            {
                collidingObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.UseOwner
                },
                topHit = new FsmBool(),
                bottomHit = new FsmBool(),
                leftHit = fsm.GetFsmBool(leftVariable),
                rightHit = fsm.GetFsmBool(rightVariable),
                topHitEvent = null,
                rightHitEvent = null,
                bottomHitEvent = null,
                leftHitEvent = null,
                otherLayer = false,
                otherLayerNumber = 0,
                ignoreTriggers = new FsmBool
                {
                    Value = true
                }
            };
        }

        public static FsmTransition TransitionToInactive()
        {
            return new FsmTransition
            {
                FsmEvent = FsmEvent.Finished,
                ToState = SsdStates.Inactive
            };
        }
    }
}