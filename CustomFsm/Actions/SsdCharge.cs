using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        private static SilkSpool Spool =>  SilkSpool.Instance;
        public override void OnEnter()
        {
            // stop the hero's animation control
            // decelerate the hero on X at 0.9, braking on exit
         
           var didAddUsingSilk =  Spool.AddUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
           Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value = didAddUsingSilk;

           // - effect  Effects/Super Jump Extra Ground Effect
           // - anim    Super Jump Antic
           // - audio   hornet_superjump_pt_1_into_position
           // - audio   hornet_superjump_pt_2_charge_2d
           // - effect  Effects/Super Jump Antic Effect L
           // - effect  Effects/Super Jump Antic Effect R
           // - anim    Super Jump Antic Effect
           // - event   FocusRumble
           // - effect  Effects/Super Jump Charging Fader

           // set the freezeCharge cState
           // turn the camera's RumblingFocus on
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

            // falling -> cancel
        }
    }
}
