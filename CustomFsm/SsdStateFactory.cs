using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Actions;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm
{
    public static class SsdStateFactory
    {
        public static FsmState Inactive(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Inactive,
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.ActivationCheck)
                });
        }
        
        public static FsmState ActivationCheck(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.ActivationCheck,
                new FsmStateAction[]
                {
                    new SsdActivationCheck()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RelinquishControl),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.Inactive)
                });
        }

        public static FsmState RelinquishControl(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.RelinquishControl,
                new FsmStateAction[]
                {
                    new SsdRelinquishControl()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.Charge)
                });
        }

        public static FsmState Charge(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Charge,
                new FsmStateAction[]
                {
                    new SsdCharge(),
                    FsmBuilder.WaitFor(fsm, SsdVars.ChargeTime, SsdEvents.Finished)
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.Charged),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.Cancelled)
                });
        }

        public static FsmState Charged(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Charged,
                new FsmStateAction[]
                {
                    new SsdCharged()
                },
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
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RegainControlToIdle)
                });
        }

        public static FsmState ResetEffects(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.ResetEffects,
                new FsmStateAction[]
                {
                    new SsdResetEffects()
                },
                new[]
                {
                    FsmBuilder.TransitionToInactive()
                });
        }

        public static FsmState GetDistance(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.GetDistance,
                new FsmStateAction[]
                {
                    new SsdGetDistance()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.ThrowNeedle, SsdStates.ThrowNeedle)
                });
        }

        public static FsmState ThrowNeedle(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.ThrowNeedle,
                new FsmStateAction[]
                {
                    new SsdThrowNeedle(),
                    FsmBuilder.WaitFor(SsdVars.NeedleThrowTimeout, SsdEvents.Finished)
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.PositionStickNeedlePre),
                    FsmBuilder.Transition(SsdEvents.DamagerHitSpikes, SsdStates.HitSpikes),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState HitSpikes(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.HitSpikes,
                new FsmStateAction[]
                {
                    new SsdHitSpikes()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState PositionStickNeedlePre(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.PositionStickNeedlePre,
                new FsmStateAction[]
                {
                    new SsdPositionStickNeedlePre()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.PositionStickNeedle),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.RegainControlToIdle),
                    FsmBuilder.Transition(SsdEvents.TransitionGate, SsdStates.HitTransitionGate),
                });
        }


        public static FsmState PositionStickNeedle(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.PositionStickNeedle,
                new FsmStateAction[]
                {
                    new SsdPositionStickNeedle()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.ThrowWait),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState HitTransitionGate(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.HitTransitionGate,
                new FsmStateAction[]
                {
                    new SsdHitTransitionGate()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RegainControlToIdle),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState ThrowWait(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.ThrowWait,
                new FsmStateAction[]
                {
                    new SsdThrowWait(),
                    FsmBuilder.WaitFor(fsm, SsdVars.ThrowWaitTime, SsdEvents.Finished)
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.DashAntic),
                });
        }

        public static FsmState DashAntic(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.DashAntic,
                new FsmStateAction[]
                {
                    new SsdDashAntic()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.DashStart),
                });
        }

        public static FsmState DashStart(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.DashStart,
                new FsmStateAction[]
                {
                    new SsdDashStart()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.Dashing),
                });
        }

        public static FsmState Dashing(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Dashing,
                new FsmStateAction[]
                {
                    FsmBuilder.CheckSides(fsm, SsdVars.WallHitLeft, SsdVars.WallHitRight),
                    new SsdDashing(),
                    new SsdApplyVelocity(),
                    FsmBuilder.WaitFor(fsm, SsdVars.CancelableTime, SsdEvents.Finished),
                    FsmBuilder.ActivateAfter(SsdObjects.ThreadLoop, SsdVars.ThreadLoopDelay),
                    new SsdThreadLoop(),
                    new SsdHitWallCheck()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.Cancelable),
                    FsmBuilder.Transition(SsdEvents.HitWall, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState Cancelable(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.Cancelable,
                new FsmStateAction[]
                {
                    FsmBuilder.CheckSides(fsm, SsdVars.WallHitLeft, SsdVars.WallHitRight),
                    new SsdCancelable(),
                    new SsdApplyVelocity(),
                    FsmBuilder.Activate(SsdObjects.ThreadLoop),
                    new SsdThreadLoop(),
                    new SsdHitWallCheck(),
                    new SsdCancelOnAction()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.HitWall, SsdStates.HitWallHard),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.RetractNeedleCancel),
                });
        }

        public static FsmState HitWallHard(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.HitWallHard,
                new FsmStateAction[]
                {
                    new SsdHitWallHard()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.HitWall),
                });
        }

        public static FsmState RetractNeedleCancel(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.RetractNeedleCancel,
                new FsmStateAction[]
                {
                    new SsdRetractNeedleCancel(),
                    FsmBuilder.MoveBy(fsm, SsdObjects.RetractNeedle, SsdVars.MoveBy, SsdVars.RetractNeedleSpeed, SsdEvents.Finished),
                    FsmBuilder.Decelerate(SsdVars.CancelDeceleration)
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.AirCancel),
                });
        }

        public static FsmState HitWall(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.HitWall,
                new FsmStateAction[]
                {
                    new SsdHitWall()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RegainControlToIdle),
                });
        }

        public static FsmState RegainControlToIdle(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.RegainControlToIdle,
                new FsmStateAction[]
                {
                    new SsdRegainControlToIdle()
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.ResetEffects),
                });
        }
        
        public static FsmState AirCancel(Fsm fsm)
        {
            return FsmBuilder.State(fsm, SsdStates.AirCancel,
                new FsmStateAction[]
                {
                    new SsdAirCancel(),
                    FsmBuilder.Decelerate(SsdVars.CancelDeceleration)
                },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Finished, SsdStates.RegainControlToIdle),
                });
        }
    }
}