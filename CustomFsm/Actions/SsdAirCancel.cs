using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirCancel : FsmStateAction
    {
        private bool _stalling;
        private float _stalled;

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            _stalling = false;
            _stalled = 0f;

            var catchEffect = SsdClones.CatchEffect;
            if (catchEffect != null)
            {
                catchEffect.gameObject.SetActive(true);
            }

            SsdEffects.PlayVoice(SsdAudio.GruntVoice);

            var needle = SsdClones.RetractNeedle;
            needle.gameObject.SetActive(false);
            needle.localPosition = new Vector3(0,SsdVars.NeedleStartHeight,0);

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            SsdHeroState.Dashing = false;

            SsdShake.Send(SsdCamera.EnemyKillShake);
        }

        public override void OnFixedUpdate()
        {
            if (_stalling)
            {
                return;
            }

            var velocity = Hero.Body.linearVelocity * SsdVars.CancelDeceleration;
            if (Mathf.Abs(velocity.x) > SsdVars.CancelStopSpeed)
            {
                Hero.Body.linearVelocity = velocity;
                return;
            }

            Hero.Body.linearVelocity = Vector2.zero;
            Hero.PlayAnim(SsdAnims.LoopCancel);
            _stalling = true;
        }

        public override void OnUpdate()
        {
            if (!_stalling)
            {
                return;
            }

            _stalled += Time.deltaTime;
            if (_stalled < SsdVars.CancelStallTime)
            {
                return;
            }

            Hero.ExitDashPose();
            Fsm.Event(SsdEvents.Finished);
        }
    }
}
