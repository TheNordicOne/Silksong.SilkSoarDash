using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPreEnteredJumping : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdPreEnteredJumping>();

        public override void OnEnter()
        {
            SsdLog.Debug("entered room exitedDashing={ExitedDashing} direction={Direction}", SsdHeroState.ExitedDashing, Fsm.GetFsmFloat(SsdVars.Direction).Value);

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