using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdLeavingScene : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdLeavingScene>();

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

            // makes the next room skip its walk-in and send ENTER SPRINTING, which SendEventSafePrefix turns into our EnterDashing
            if (SsdHeroState.Dashing)
            {
                HeroController.instance.exitedSprinting = true;
            }

            SsdLog.Debug("leaving from={State} dashing={Dashing}", previous.Name, SsdHeroState.Dashing);
            Finish();
        }
    }
}