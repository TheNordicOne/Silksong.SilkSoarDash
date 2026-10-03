using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRetractNeedleCancel : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdRetractNeedleCancel>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;


            SsdEffects.PlayOneShot(SsdAudio.Cancel, SsdAudio.NarrowPitchMin, SsdAudio.NarrowPitchMax);
            
            RetractNeedle();

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);
        }

        private void RetractNeedle()
        {
            var needle = SsdClones.RetractNeedle;
            var damager = SsdClones.Damager;
            
            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;

            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var distance = -SsdVars.RetractNeedleDistance * direction;

            Fsm.GetFsmFloat(SsdVars.Distance).Value = distance;
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(distance, 0f, 0f);

            SsdLog.Debug("retracting distance={Distance} direction={Direction}", distance, direction);

            needle.gameObject.SetActive(true);
            stickNeedle.SetActive(false);
            damager.gameObject.SetActive(false);
        }
    }
}
