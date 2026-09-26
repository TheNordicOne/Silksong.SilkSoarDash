using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharged : FsmStateAction
    {
        public override void OnEnter()
        {
            // - effect  Hornet_Super_Jump_Ready_Burst
            // - audio   hornet_superjump_pt_3_charge_ready
            // - audio   Nail Art Ready
            // - audio   hornet_dramatic_stance_crazy_cloak_loop

            var chargedEffect = HeroController.instance.transform.Find("Effects/Super Jump Charged");
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(true);
            }

            // - flash  FlashingSuperDash
            // - event  AverageShake
        }

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
            
            Fsm.Event(SsdEvents.GetDistance);
            Finish();
        }
    }
}
