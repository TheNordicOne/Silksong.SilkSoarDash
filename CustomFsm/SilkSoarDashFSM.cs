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
                    }
                }
            };

            fsm.States = new[]
            {
                SsdStateFactory.Inactive(fsm),
                SsdStateFactory.RelinquishControl(fsm),
                SsdStateFactory.Charge(fsm),
                SsdStateFactory.Charged(fsm),
                SsdStateFactory.Cancelled(fsm),
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
            };

            fsm.StartState = SsdStates.Inactive;

            _host = hero.gameObject.AddComponent<PlayMakerFSM>();
            _host.Fsm = fsm;
            fsm.Init(_host);
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
    }
}