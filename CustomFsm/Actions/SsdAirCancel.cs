using GlobalEnums;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirCancel : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
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

            Hero.ExitDashPose();

            ConsumeCancelPress();

            Finish();
        }

        // control returns within the 0.1s input buffer, so the cancel press would still fire its own action
        private static void ConsumeCancelPress()
        {
            Hero.ResetInputQueues();
            Hero.ClearJumpInputState();

            var input = GameManager.instance.inputHandler;
            input.GetWasButtonPressedQueued(HeroActionButton.JUMP, true);
            input.GetWasButtonPressedQueued(HeroActionButton.ATTACK, true);
            input.GetWasButtonPressedQueued(HeroActionButton.SUPER_DASH, true);
        }
    }
}
