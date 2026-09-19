using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharged : FsmStateAction
    {
        
        private const string ReleaseEvent = "GET_DISTANCE";
        
        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            if (ia.SuperDash.IsPressed)
            {
                return;
            }

            Fsm.Event(ReleaseEvent);
            Finish();
        }
    }
}
