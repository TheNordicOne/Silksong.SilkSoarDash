using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirCatch : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value.SetActive(false);
            SsdClones.Damager.gameObject.SetActive(false);

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            SsdHeroState.Dashing = false;

            // keeping the leading edge in place stops the pose reset from pulling her back
            Hero.ExitDashPoseAtWall(Fsm.GetFsmFloat(SsdVars.Direction).Value);
            Hero.AffectedByGravity(true);
            Hero.PlayAnim(SsdAnims.WallCatch);
            SsdClones.GrabEffect.gameObject.SetActive(true);

            SsdShake.Send(SsdCamera.EnemyKillShake);
            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);
        }

        public override void OnUpdate()
        {
            Hero.ClampFall();

            if (Hero.IsAnimPlaying(SsdAnims.WallCatch))
            {
                return;
            }

            Fsm.Event(SsdEvents.Finished);
        }
    }
}
