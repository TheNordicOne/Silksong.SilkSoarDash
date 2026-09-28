using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPreEnteredJumping : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!SsdHeroState.ExitedDashing)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdHeroState.ExitedDashing = false;

            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;
            SsdHeroState.Dashing = true;
            Finish();
        }
    }
}