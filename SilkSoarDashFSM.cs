using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash
{
    public static class SilkSoarDashFsm
    {
        private static PlayMakerFSM _probe;

        public static void Start()
        {
            if (_probe != null)
            {
                return;
            }

            var go = new GameObject("SSD_FsmProbe");
            Object.DontDestroyOnLoad(go);
            _probe = go.AddComponent<PlayMakerFSM>();

            var fsm = new Fsm
            {
                Name = "SilkSoarDash"
            };

            var tick = new FsmEvent("TICK");

            // states
            // fsm.States = new[] { };

            fsm.StartState = "SsdCharge";

            _probe.Fsm = fsm;
            fsm.Init(_probe);
            fsm.Start();
        }
    }
}