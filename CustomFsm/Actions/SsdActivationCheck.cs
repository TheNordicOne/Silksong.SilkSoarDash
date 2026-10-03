using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.Config;
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
            // fixed for the whole soar, so a config change mid-soar cannot unbalance the silk it reserved
            SsdHeroState.SilkCost = SsdConfig.SilkCost;

            var silk = Hero.playerData.silk;
            var hasEnoughSilk = silk >= SsdHeroState.SilkCost;

            if (!hasEnoughSilk)
            {
                SsdLog.Debug("cancelled silk={Silk} cost={Cost}", silk, SsdHeroState.SilkCost);
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdLog.Debug("charging silk={Silk}", silk);

            Finish();
        }
    }
}
