using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitSpikes : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdHitSpikes>();

        
        public override void OnEnter()
        {
            SsdLog.Debug("spikes object={Object}", Fsm.GetFsmGameObject(SsdVars.HitObject).Value);
            Fsm.GetFsmBool(SsdVars.DidHit).Value = false;

            SsdEffects.Shake(SsdCamera.SmallShake);
            SsdEffects.PlayTableAt(SsdAudio.TinkEffect, SsdClones.ThrowNeedle.position);

            Finish();
        }
    }
}