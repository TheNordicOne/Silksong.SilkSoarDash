using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelRumblingFocus : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!Fsm.GetFsmBool(SsdVars.StartedRumblingFocus).Value)
            {
                Finish();
                return;
            }

            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus).Value = false;

            // turn the camera's RumblingFocus off

            Finish();
        }
    }
}
