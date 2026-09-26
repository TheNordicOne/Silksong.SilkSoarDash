using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        public override void OnEnter()
        {
            // 1  play the throw wait animation
            //    - anim  Super Jump Throw Wait
            // 2  read isNeedleVisible
            var isNeedleVisible = !Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value;

            // 3  ThrowWaitTime = 0 if isNeedleVisible, 0.5 otherwise
            Fsm.GetFsmFloat(SsdVars.ThrowWaitTime).Value = isNeedleVisible ? 0f : SsdVars.ThrowWaitTimeOffScreen;

            Finish();
        }
    }
}