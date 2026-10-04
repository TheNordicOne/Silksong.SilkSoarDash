using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.Controls;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelOnAction : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCancelOnAction>();

        public override void OnUpdate()
        {
            if (!SsdInput.PressedCancel())
            {
                return;
            }

            SsdLog.Debug("cancelled button={Button}", PressedButton(SsdInput.Actions));
            Fsm.Event(SsdEvents.Cancelled);
        }

        private static string PressedButton(HeroActions actions)
        {
            if (actions.Jump.WasPressed)
            {
                return "jump";
            }

            return actions.Attack.WasPressed ? "attack" : "superdash";
        }
    }
}