using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

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

            SsdLog.Debug("cancelled button={Button}", PressedButton());
            Fsm.Event(SsdEvents.Cancelled);
        }

        private static string PressedButton()
        {
            if (InputAction.Jump.WasPressed)
            {
                return "jump";
            }

            return InputAction.Attack.WasPressed ? "attack" : "superdash";
        }
    }
}