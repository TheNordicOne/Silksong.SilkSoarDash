using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm;
using SilkSoarDash.CustomFsm.Actions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SilkSoarDash
{
    public static class SilkSoarDashFsm
    {
        private static PlayMakerFSM _host;

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Build()
        {
            if (_host != null)
            {
                return;
            }

            var go = new GameObject("SSD_Fsm");
            Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<PlayMakerFSM>();

            var fsm = new Fsm
            {
                Name = "SilkSoarDash"
            };


            var inactive = FsmBuilder.State(fsm, SsdStates.Inactive, new[] { FsmBuilder.Transition(SsdEvents.Start, SsdStates.Charge) });


            var charge = FsmBuilder.State(fsm, SsdStates.Charge, new FsmStateAction[] { new SsdCharge() },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.Charged, SsdStates.Charged),
                    FsmBuilder.Transition(SsdEvents.Cancelled, SsdStates.Cancelled)
                });

            var chargedState = FsmBuilder.State(fsm, SsdStates.Charged, new FsmStateAction[] { new SsdCharged() },
                new[]
                {
                    FsmBuilder.Transition(SsdEvents.GetDistance, SsdStates.GetDistance)
                });
            
            var cancelledState = FsmBuilder.State(fsm, SsdStates.Cancelled, new[] { FsmBuilder.TransitionToInactive() });

            var gettingDistance = FsmBuilder.State(fsm, SsdStates.GetDistance, new[] { FsmBuilder.TransitionToInactive() });

            fsm.States = new[] { inactive, charge, chargedState, cancelledState, gettingDistance };
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
    }
}