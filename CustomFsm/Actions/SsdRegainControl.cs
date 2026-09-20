using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRegainControl : FsmStateAction
    {
        public override void OnEnter()
        {
            var hero = HeroController.instance;
            if (hero == null)
            {
                Finish();
                return;
            }

            hero.RegainControl();
            hero.StartAnimationControlToIdle();

            var chargedEffect = hero.transform.Find("Effects/Super Jump Charged");
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(false);
            }

            Finish();
        }
    }
}
