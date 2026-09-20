using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharged : FsmStateAction
    {
        public override void OnEnter()
        {
            var chargedEffect = HeroController.instance.transform.Find("Effects/Super Jump Charged");
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(true);
            }
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
