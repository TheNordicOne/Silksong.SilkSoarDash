using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdBeginJumping : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdBeginJumping>();

        public override void OnEnter()
        {
            // the dash direction from the previous room, the game may turn her on entry
            var dir = new Vector2(Fsm.GetFsmFloat(SsdVars.Direction).Value, 0f);
            SsdRayCast.CastAndStore(Fsm, dir, State);
            SsdLog.Debug("measured again direction={Direction} hit={Hit} distance={Distance} object={Object} gate={Gate}", dir.x, Fsm.GetFsmBool(SsdVars.DidHit).Value, Fsm.GetFsmFloat(SsdVars.Distance).Value, Fsm.GetFsmGameObject(SsdVars.HitObject).Value, Fsm.GetFsmBool(SsdVars.IsGate).Value);
            Finish();
        }
    }
}