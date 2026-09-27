using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

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
                SsdLog.LogDebug("cancelled, silk " + silk + " below cost " + SsdVars.SilkCost);
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdLog.LogDebug("silk " + silk);

            Finish();
        }
    }
}
