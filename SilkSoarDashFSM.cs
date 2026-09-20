using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SilkSoarDash
{
    public static class SilkSoarDashFsm
    {
        private const string HostObjectName = "SSD_Fsm";
        private const string FsmName = "SilkSoarDash";

        private static PlayMakerFSM _host;

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Build()
        {
            if (_host != null)
            {
                return;
            }

            _host = CreateHost();

            var fsm = new Fsm
            {
                Name = FsmName,
                Variables =
                {
                    FloatVariables = new[] { new FsmFloat(SsdVars.Distance), new FsmFloat(SsdVars.Direction) },
                    Vector2Variables = new[] { new FsmVector2(SsdVars.HitPoint) },
                    Vector3Variables = new[] { new FsmVector3(SsdVars.MoveBy) },
                    GameObjectVariables = new[] { new FsmGameObject(SsdVars.HitObject) },
                    BoolVariables = new[]
                    {
                        new FsmBool(SsdVars.DidHit),
                        new FsmBool(SsdVars.IsGate),
                        new FsmBool(SsdVars.HitSpikes)
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
                SsdStateFactory.ThrowNeedle(fsm)
            };
            
            fsm.StartState = SsdStates.Inactive;

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

        private static PlayMakerFSM CreateHost()
        {
            var go = new GameObject(HostObjectName);
            Object.DontDestroyOnLoad(go);
            return go.AddComponent<PlayMakerFSM>();
        }
    }
}
