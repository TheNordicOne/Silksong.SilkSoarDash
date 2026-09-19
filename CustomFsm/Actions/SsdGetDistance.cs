using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdGetDistance : FsmStateAction
    {

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            Fsm.Event(SsdEvents.ThrowNeedle);
            Finish();
        }
    }
}
