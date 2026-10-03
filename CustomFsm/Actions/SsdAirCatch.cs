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

            SsdHeroState.Dashing = false;

            Hero.ExitDashPoseKeepingFront(Fsm.GetFsmFloat(SsdVars.Direction).Value);
            Hero.AffectedByGravity(true);
            Hero.PlayAnim(SsdAnims.Catch);
            SsdClones.GrabEffect.gameObject.SetActive(true);

            SsdShake.Send(SsdCamera.EnemyKillShake);
            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);
        }

        public override void OnUpdate()
        {
            Hero.ClampFall();

            if (Hero.IsAnimPlaying(SsdAnims.Catch))
            {
                return;
            }

            Fsm.Event(SsdEvents.Finished);
        }
    }
}
