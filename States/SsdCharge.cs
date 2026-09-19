using HutongGames.PlayMaker;

namespace SilkSoarDash.States
{
    public class SsdCharge : FsmStateAction
    {
        private static readonly BepInEx.Logging.ManualLogSource SsdChargeLog = BepInEx.Logging.Logger.CreateLogSource("SsdCharge");
        

        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            SsdChargeLog.LogInfo("SsdCharge  OnEnter");
        }

        public override void OnUpdate()
        {
            Finish();
        }

        public override void OnExit()
        {
            SsdChargeLog.LogInfo("SsdCharge  OnExit");
        }
    }
}