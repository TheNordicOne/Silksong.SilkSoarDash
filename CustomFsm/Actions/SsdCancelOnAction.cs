using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelOnAction : FsmStateAction
    {
        private static HeroActions InputAction => GameManager.instance?.inputHandler?.inputActions;
        
        public override void OnUpdate()
        {
            if (InputAction == null)
            {
                return;
            }
            
            if (InputAction.Jump.WasPressed || InputAction.Attack.WasPressed || InputAction.SuperDash.WasPressed )
            {
                Fsm.Event(SsdEvents.Cancelled);
            }
        }
    }
}