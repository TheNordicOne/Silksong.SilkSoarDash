using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdGetDistance : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdGetDistance>();
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var dir = Hero.cState.facingRight ? Vector2.right : Vector2.left;

            Fsm.GetFsmFloat(SsdVars.Distance).Value = SsdVars.DefaultThrowDistance;
            SsdRayCast.CastAndStore(Fsm, dir, State);

            var dist = Fsm.GetFsmFloat(SsdVars.Distance).Value;
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(dist * dir.x, 0f, 0f);

            SsdLog.Debug("measured direction={Direction} hit={Hit} distance={Distance} object={Object} gate={Gate} spikes={Spikes}", dir.x, Fsm.GetFsmBool(SsdVars.DidHit).Value, dist, Fsm.GetFsmGameObject(SsdVars.HitObject).Value, Fsm.GetFsmBool(SsdVars.IsGate).Value, Fsm.GetFsmBool(SsdVars.HitSpikes).Value);

            Fsm.Event(SsdEvents.ThrowNeedle);
            Finish();
        }
    }
}