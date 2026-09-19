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


            var inactive = FsmBuilder.State(fsm, "Inactive", new[] { FsmBuilder.Transition("START", "SsdCharge") });


            var charge = FsmBuilder.State(fsm, "SsdCharge", new FsmStateAction[] { new SsdCharge() },
                new[]
                {
                    FsmBuilder.Transition("CHARGED", "Charged"),
                    FsmBuilder.Transition("CANCELLED", "Cancelled")
                });

            charge.SaveActions();
            
            var chargedState = FsmBuilder.State(fsm, "Charged", new[] { FsmBuilder.TransitionToInactive() });
            var cancelledState = FsmBuilder.State(fsm, "Cancelled", new[] { FsmBuilder.TransitionToInactive() });
            
            fsm.States = new[] { inactive, charge, chargedState, cancelledState };
            fsm.StartState = "Inactive";

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

            _host.Fsm.Event("START");
        }
    }
}