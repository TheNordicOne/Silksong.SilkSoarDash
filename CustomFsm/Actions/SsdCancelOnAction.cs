using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelOnAction : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCancelOnAction>();

        private static HeroActions InputAction => GameManager.instance?.inputHandler?.inputActions;
        
        public override void OnUpdate()
        {
            if (InputAction == null)
            {
                return;
            }
            
            if (!InputAction.Jump.WasPressed && !InputAction.Attack.WasPressed && !InputAction.SuperDash.WasPressed)
            {
                return;
            }

            SsdLog.LogDebug("cancelled, jump " + InputAction.Jump.WasPressed + ", attack " + InputAction.Attack.WasPressed + ", superdash " + InputAction.SuperDash.WasPressed);
            Fsm.Event(SsdEvents.Cancelled);
        }
    }
}