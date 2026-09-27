using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

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

        public static ActivateGameObject Activate(Transform target)
        {
            return new ActivateGameObject
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
                recursive = new FsmBool
                {
                    Value = false
                },
                resetOnExit = true,
                everyFrame = false
            };
        }

        public static ActivateGameObjectDelay ActivateAfter(Transform target, float delay)
        {
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

        public static iTweenMoveBy MoveBy(Fsm fsm, Transform target, string vectorVariable, float speed, string finishEvent)
        {
            return new iTweenMoveBy
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = new FsmGameObject
                    {
                        Value = target == null ? null : target.gameObject
                    }
                },
                id = new FsmString(),
                vector = fsm.GetFsmVector3(vectorVariable),
                time = new FsmFloat
                {
                    Value = 0f
                },
                delay = new FsmFloat
                {
                    Value = 0f
                },
                speed = new FsmFloat
                {
                    Value = speed
                },
                easeType = iTween.EaseType.linear,
                loopType = iTween.LoopType.none,
                space = Space.World,
                orientToPath = new FsmBool
                {
                    Value = false
                },
                lookAtObject = new FsmGameObject
                {
                    UseVariable = true
                },
                lookAtVector = new FsmVector3{
                    UseVariable = true
                },
                lookTime = new FsmFloat
                {
                    Value = 0f
                },
                axis = iTweenFsmAction.AxisRestriction.none,
                startEvent = null,
                finishEvent = new FsmEvent(finishEvent),
                realTime = new FsmBool
                {
                    Value = false
                },
                stopOnExit = new FsmBool
                {
                    Value = true
                },
                loopDontFinish = new FsmBool
                {
                    Value = false
                }
            };
        }

        public static DecelerateXY DecelerateAxes(float decelerationX, float decelerationY, bool brakeOnExit)
        {
            return new DecelerateXY
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.UseOwner
                },
                decelerationX = new FsmFloat
                {
                    Value = decelerationX
                },
                decelerationY = new FsmFloat
                {
                    Value = decelerationY
                },
                brakeOnExit = brakeOnExit
            };
        }

        public static DecelerateV2 Decelerate(float deceleration)
        {
            return new DecelerateV2
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.UseOwner
                },
                deceleration = new FsmFloat
                {
                    Value = deceleration
                },
                brakeOnExit = false
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