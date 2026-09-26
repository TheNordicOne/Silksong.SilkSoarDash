using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        public override void OnEnter()
        {
            // anim  Super Jump Throw Wait

            var isNeedleVisible = !Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value;
            Fsm.GetFsmFloat(SsdVars.ThrowWaitTime).Value = isNeedleVisible ? 0f : SsdVars.ThrowWaitTimeOffScreen;

            Finish();
        }
    }
}