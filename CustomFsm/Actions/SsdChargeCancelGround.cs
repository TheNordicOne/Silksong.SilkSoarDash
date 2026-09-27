using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdChargeCancelGround : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool => SilkSpool.Instance;

        public override void OnEnter()
        {
            if (Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value)
            {
                Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
            }

            // effect  Effects/Super Jump Extra Ground Effect off
            // audio   stop the charge loop
            // effect  Effects/Super Jump Antic Effect L off
            // effect  Effects/Super Jump Antic Effect R off
            // effect  Effects/Super Jump Charging Fader to alpha 0 over 0.1

            Hero.SetCState(SsdCStates.FreezeCharge, false);

            // anim  Super Jump Antic Cancel
            // anim  Super Jump Antic Effect End on both antic effects

            // turn the camera's RumblingFocus and RumblingFocus2 off

            Hero.Body.linearVelocity = Vector2.zero;
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Antic Cancel animation completes
            Finish();
        }
    }
}
