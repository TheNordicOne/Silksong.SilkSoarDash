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

            var fsm = new Fsm { Name = FsmName };

            fsm.States = new[]
            {
                SsdStateFactory.Inactive(fsm),
                SsdStateFactory.Charge(fsm),
                SsdStateFactory.Charged(fsm),
                SsdStateFactory.Cancelled(fsm),
                SsdStateFactory.GetDistance(fsm)
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
