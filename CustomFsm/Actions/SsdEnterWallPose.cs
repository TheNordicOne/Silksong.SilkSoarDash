using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdEnterWallPose : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            // a sliding Hornet faces the wall, so the dash goes the other way
            var direction = Hero.cState.facingRight ? -1f : 1f;
            Fsm.GetFsmFloat(SsdVars.Direction).Value = direction;

            SsdHeroState.WallStart = true;
            SsdHeroState.WallStartPosition = Hero.transform.position;

            Hero.EnterWallPose(direction);
            Hero.AffectedByGravity(false);
            Hero.Body.linearVelocity = Vector2.zero;

            SsdClones.AimForTurnedHero();

            Finish();
        }
    }
}
