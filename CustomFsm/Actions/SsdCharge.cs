using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        private static readonly BepInEx.Logging.ManualLogSource SsdChargeLog = BepInEx.Logging.Logger.CreateLogSource("SsdCharge");

        private const float ChargeTime = 0.8f;

        private float _elapsed;

        public override void Reset()
        {
            _elapsed = 0f;
        }

        public override void OnEnter()
        {
            _elapsed = 0f;
            SsdChargeLog.LogInfo("OnEnter - charging for " + ChargeTime + "s");
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            if (!ia.SuperDash.IsPressed)
            {
                SsdChargeLog.LogInfo("released early at " + _elapsed.ToString("F2") + "s -> " + SsdEvents.Cancelled);
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            _elapsed += Time.deltaTime;

            if (!(_elapsed >= ChargeTime))
            {
                return;
            }
            
            Finish();
        }

        public override void OnExit()
        {
            SsdChargeLog.LogInfo("OnExit after " + _elapsed.ToString("F2") + "s");
        }
    }
}
