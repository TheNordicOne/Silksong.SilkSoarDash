using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm
{
    public static class SilkSoarDashFsm
    {
        private const string FsmName = "SilkSoarDash";
        private static PlayMakerFSM _host;

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Build()
        {
            if (_host != null)
            {
                return;
            }

            var hero = HeroController.instance;
            if (hero == null)
            {
                return;
            }

            SsdClones.Build(hero);
            SsdEffects.Build(hero);

            var fsm = new Fsm
            {
                Name = FsmName,
                Variables =
                {
                    FloatVariables = new[]
                    {
                        new FsmFloat(SsdVars.Distance),
                        new FsmFloat(SsdVars.Direction),
                        new FsmFloat(SsdVars.CancelableTime),
                        new FsmFloat(SsdVars.JumpSpeed)
                        {
                            Value = SsdVars.DefaultJumpSpeed
                        },
                        new FsmFloat(SsdVars.ChargeTime)
                        {
                            Value = SsdVars.DefaultChargeTime
                        },
                        new FsmFloat(SsdVars.ThrowWaitTime)
                    },
                    Vector2Variables = new[] { new FsmVector2(SsdVars.HitPoint) },
                    Vector3Variables = new[] { new FsmVector3(SsdVars.MoveBy) },
                    GameObjectVariables = new[]
                    {
                        new FsmGameObject(SsdVars.HitObject),
                        new FsmGameObject(SsdVars.StickNeedle),
                        new FsmGameObject(SsdVars.StickNeedleParent)
                    },
                    BoolVariables = new[]
                    {
                        new FsmBool(SsdVars.DidHit),
                        new FsmBool(SsdVars.IsGate),
                        new FsmBool(SsdVars.HitSpikes),
                        new FsmBool(SsdVars.NeedleOffScreen),
                        new FsmBool(SsdVars.QueuedCancel),
                        new FsmBool(SsdVars.DidAddUsingSilk),
                        new FsmBool(SsdVars.DidStartFlash),
                        new FsmBool(SsdVars.StartedRumblingFocus),
                        new FsmBool(SsdVars.StartedRumblingFocus2),
                        new FsmBool(SsdVars.AirTarget)
                    }
                }
            };

            fsm.States = new[]
            {
                SsdStateFactory.Inactive(fsm),
                SsdStateFactory.ActivationCheck(fsm),
                SsdStateFactory.RelinquishControl(fsm),
                SsdStateFactory.Charge(fsm),
                SsdStateFactory.Charged(fsm),
                SsdStateFactory.ThrowNeedleStart(fsm),
                SsdStateFactory.GetDistance(fsm),
                SsdStateFactory.ThrowNeedle(fsm),
                SsdStateFactory.ResetEffects(fsm),
                SsdStateFactory.HitSpikes(fsm),
                SsdStateFactory.PositionStickNeedlePre(fsm),
                SsdStateFactory.PositionStickNeedle(fsm),
                SsdStateFactory.HitTransitionGate(fsm),
                SsdStateFactory.ThrowWait(fsm),
                SsdStateFactory.DashAntic(fsm),
                SsdStateFactory.DashStart(fsm),
                SsdStateFactory.Dashing(fsm),
                SsdStateFactory.Cancelable(fsm),
                SsdStateFactory.HitWallHard(fsm),
                SsdStateFactory.RetractNeedleCancel(fsm),
                SsdStateFactory.HitWall(fsm),
                SsdStateFactory.AirCatch(fsm),
                SsdStateFactory.AirCatchRegainControl(fsm),
                SsdStateFactory.RegainControlToIdle(fsm),
                SsdStateFactory.AirCancel(fsm),
                SsdStateFactory.Cancel(fsm),
                SsdStateFactory.CancelRumblingFocus(fsm),
                SsdStateFactory.CancelRumblingFocus2(fsm),
                SsdStateFactory.ChargeCancelGround(fsm),
                SsdStateFactory.LeavingScene(fsm),
                SsdStateFactory.PreEnteredJumping(fsm),
                SsdStateFactory.EnteredJumping(fsm),
                SsdStateFactory.BeginJumping(fsm),
                SsdStateFactory.PositionStickNeedlePre2(fsm),
                SsdStateFactory.HitTransitionGate2(fsm),
                SsdStateFactory.QueueCancel(fsm),
                SsdStateFactory.DashStartQuick(fsm)
            };

            fsm.GlobalTransitions = new[]
            {
                FsmBuilder.Transition(SsdEvents.HeroDamaged, SsdStates.Cancel),
                FsmBuilder.Transition(SsdEvents.FsmCancel, SsdStates.Cancel),
                FsmBuilder.Transition(SsdEvents.LeavingScene, SsdStates.LeavingScene),
                FsmBuilder.Transition(SsdEvents.EnterDashing, SsdStates.PreEnteredJumping)
            };

            fsm.StartState = SsdStates.Inactive;

            _host = hero.gameObject.AddComponent<PlayMakerFSM>();
            _host.Fsm = fsm;
            fsm.Init(_host);
            _host.AddEventHandlerComponents();
            fsm.Start();
        }

        public static void Trigger()
        {
            if (!_host)
            {
                return;
            }

            _host.Fsm.Event(SsdEvents.Start);
        }

        public static void EnterDashing()
        {
            if (!_host)
            {
                return;
            }

            _host.Fsm.Event(SsdEvents.EnterDashing);
        }
    }
}