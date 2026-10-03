using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRelinquishControl : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            Hero.RelinquishControl();
            Hero.StopAnimationControl();

            Finish();
        }
    }
}