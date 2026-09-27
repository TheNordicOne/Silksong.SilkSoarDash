using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelRumblingFocus2 : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!Fsm.GetFsmBool(SsdVars.StartedRumblingFocus2).Value)
            {
                Finish();
                return;
            }

            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus2).Value = false;

            SsdShake.SetFocus2(false);

            Finish();
        }
    }
}
