using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWall : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var damager = Hero.transform.Find(SsdObjects.Damager);
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            Hero.SetCState(SsdCStates.SuperDashOnWall, true);
            Hero.SetCState(SsdCStates.SuperDashing, false);
            Hero.AffectedByGravity(false);

            // anim  HeroAnimationController.SetPlaySuperJumpFall()

            Hero.Body.linearVelocity = Vector2.zero;
        }

        public override void OnUpdate()
        {
            // finish when the current animation completes
            Finish();
        }
    }
}