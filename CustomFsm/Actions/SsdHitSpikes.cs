using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitSpikes : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdHitSpikes>();

        
        public override void OnEnter()
        {
            SsdLog.LogDebug("needle hit spikes");
            Fsm.GetFsmBool(SsdVars.DidHit).Value = false;

            // - shake  Small Shake
            // - audio  tink_effect

            Finish();
        }
    }
}