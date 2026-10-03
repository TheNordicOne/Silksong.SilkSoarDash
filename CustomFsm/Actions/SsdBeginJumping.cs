using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdBeginJumping : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdBeginJumping>();

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            // the game drops her at the door's floor, but she flies at the height she left with, so measure from there
            var lowest = Hero.transform.position.y + SsdVars.KickUpHeight;
            Hero.SetHeightAboveDoorFloor(Hero.sceneEntryGate, SsdHeroState.ExitHeight, lowest);

            // the dash direction from the previous room, the game may turn her on entry
            var dir = new Vector2(Fsm.GetFsmFloat(SsdVars.Direction).Value, 0f);
            SsdRayCast.CastAndStore(Fsm, dir, State);
            SsdLog.Debug("measured again direction={Direction} hit={Hit} distance={Distance} object={Object} gate={Gate} spikes={Spikes} height={Height}", dir.x, Fsm.GetFsmBool(SsdVars.DidHit).Value, Fsm.GetFsmFloat(SsdVars.Distance).Value, Fsm.GetFsmGameObject(SsdVars.HitObject).Value, Fsm.GetFsmBool(SsdVars.IsGate).Value, Fsm.GetFsmBool(SsdVars.HitSpikes).Value, Hero.transform.position.y);
            Finish();
        }
    }
}