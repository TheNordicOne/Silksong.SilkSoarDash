using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdStateTrace : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdStateTrace>();

        public override void OnEnter()
        {
            var previous = Fsm.PreviousActiveState == null ? "none" : Fsm.PreviousActiveState.Name;
            var via = Fsm.LastTransition == null ? "none" : Fsm.LastTransition.EventName;
            SsdLog.Debug("enter {State} from={Previous} via={Event}", State.Name, previous, via);
            Finish();
        }
    }
}