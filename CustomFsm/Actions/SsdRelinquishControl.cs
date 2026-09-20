using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRelinquishControl : FsmStateAction
    {
        public override void OnEnter()
        {
            var hero = HeroController.instance;
            if (hero != null)
            {
                hero.RelinquishControl();
                hero.StopAnimationControl();
            }

            Finish();
        }
    }
}
