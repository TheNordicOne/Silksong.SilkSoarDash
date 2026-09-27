using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdActivationCheck : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            var hasEnoughSilk = Hero.playerData.silk >= SsdVars.SilkCost;
            if (!hasEnoughSilk)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            Finish();
        }
    }
}
