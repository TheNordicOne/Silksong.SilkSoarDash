using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdLeavingScene : FsmStateAction
    {
        public override void OnEnter()
        {
            // the game sends this on every room change, so ignore it while the ability is not running
            var previous = Fsm.PreviousActiveState;
            if (previous == null || previous.Name == SsdStates.Inactive)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdHeroState.ExitedDashing = SsdHeroState.Dashing;
            Finish();
        }
    }
}