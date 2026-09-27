using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdThrowWait>();

        public override void OnEnter()
        {
            // anim  Super Jump Throw Wait

            var isNeedleVisible = !Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value;
            var waitTime = isNeedleVisible ? 0f : SsdVars.ThrowWaitTimeOffScreen;
            Fsm.GetFsmFloat(SsdVars.ThrowWaitTime).Value = waitTime;

            SsdLog.LogDebug("needle visible " + isNeedleVisible + ", waiting " + waitTime);

            Finish();
        }
    }
}