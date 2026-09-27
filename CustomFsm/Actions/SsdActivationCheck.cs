using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdActivationCheck : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdActivationCheck>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            var silk = Hero.playerData.silk;
            var hasEnoughSilk = silk >= SsdVars.SilkCost;

            if (!hasEnoughSilk)
            {
                SsdLog.Debug("cancelled silk={Silk} cost={Cost}", silk, SsdVars.SilkCost);
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdLog.Debug("charging silk={Silk}", silk);

            Finish();
        }
    }
}
