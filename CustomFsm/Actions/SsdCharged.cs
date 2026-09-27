using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharged : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool => SilkSpool.Instance;

        public override void OnEnter()
        {
            
            Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
            Hero.TakeSilk(SsdVars.SilkCost);

            // - effect  Hornet_Super_Jump_Ready_Burst
            // - audio   hornet_superjump_pt_3_charge_ready
            // - audio   Sounds/Nail Art Ready
            // - audio   hornet_dramatic_stance_crazy_cloak_loop

            var chargedEffect = HeroController.instance.transform.Find(SsdObjects.ChargedEffect);
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(true);
            }

            // - flash  FlashingSuperDash, keeping its id and setting DidStartFlash
            // - event  AverageShake

            // turn the camera's RumblingFocus off and RumblingFocus2 on, clearing StartedRumblingFocus and setting StartedRumblingFocus2
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }
            
            if (Hero.IsFalling())
            {
                Fsm.Event(SsdEvents.GetDistance);
                return;
            }

            if (ia.SuperDash.IsPressed)
            {
                return;
            }


            Fsm.Event(SsdEvents.GetDistance);
        }
    }
}