using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCharge>();

        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool =>  SilkSpool.Instance;
        
        public override void OnEnter()
        {
            Hero.StopAnimationControl();
         
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
           
           Hero.SetCState(SsdCStates.FreezeCharge, true);
           
           // turn the camera's RumblingFocus on, and set StartedRumblingFocus
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            var released = !ia.SuperDash.IsPressed;
            var falling = Hero.IsFalling();

            if (released || falling)
            {
                SsdLog.LogDebug("cancelled, released " + released + ", falling " + falling);
                Fsm.Event(SsdEvents.Cancelled);
            }
            
        }
    }
}
