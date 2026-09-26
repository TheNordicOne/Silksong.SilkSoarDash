using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        public override void OnEnter()
        {
            // - effect  Effects/Super Jump Extra Ground Effect
            // - anim    Super Jump Antic
            // - audio   hornet_superjump_pt_1_into_position
            // - audio   hornet_superjump_pt_2_charge_2d
            // - effect  Effects/Super Jump Antic Effect L
            // - effect  Effects/Super Jump Antic Effect R
            // - anim    Super Jump Antic Effect
            // - event   FocusRumble
            // - effect  Effects/Super Jump Charging Fader
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            if (!ia.SuperDash.IsPressed)
            {
                Fsm.Event(SsdEvents.Cancelled);
            }
        }
    }
}
