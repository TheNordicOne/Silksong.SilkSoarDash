using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdBeginJumping : FsmStateAction
    {
        public override void OnEnter()
        {
            // the dash direction from the previous room, the game may turn her on entry
            var dir = new Vector2(Fsm.GetFsmFloat(SsdVars.Direction).Value, 0f);
            SsdRayCast.CastAndStore(Fsm, dir, State);
            Finish();
        }
    }
}