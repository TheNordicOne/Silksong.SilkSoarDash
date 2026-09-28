using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWall : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var damager = SsdClones.Damager;
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            SsdHeroState.OnWall = true;
            SsdHeroState.Dashing = false;
            Hero.AffectedByGravity(true);

            Hero.PlayAnim(SsdAnims.WallCatch);
            SsdClones.GrabEffect.gameObject.SetActive(true);

            Hero.Body.linearVelocity = Vector2.zero;
        }

        public override void OnUpdate()
        {
            ClampFall();

            if (Hero.IsAnimPlaying(SsdAnims.WallCatch))
            {
                return;
            }

            Finish();
        }

        private static void ClampFall()
        {
            var velocity = Hero.Body.linearVelocity;

            Hero.Body.linearVelocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -SsdVars.CatchFallSpeed, SsdVars.CatchFallSpeed));
        }
    }
}