using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdThrowWait>();

        public override void OnEnter()
        {

            var isNeedleVisible = !Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value;
            var waitTime = isNeedleVisible ? 0f : SsdVars.ThrowWaitTimeOffScreen;
            Fsm.GetFsmFloat(SsdVars.ThrowWaitTime).Value = waitTime;

            SsdLog.Debug("waiting visible={Visible} seconds={Seconds}", isNeedleVisible, waitTime);

            Finish();
        }
    }
}