using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRetractNeedleCancel : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdRetractNeedleCancel>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            Fsm.GetFsmBool(SsdVars.QueuedCancel).Value = false;

            // audio  hornet_superjump_cancel
            
            RetractNeedle();

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);
        }

        private void RetractNeedle()
        {
            var needle = Hero.transform.Find(SsdObjects.RetractNeedle);
            var damager = Hero.transform.Find(SsdObjects.Damager);
            
            var stickNeedle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var needleChild = Hero.transform.Find(SsdObjects.RetractNeedleChild);
            
            var needleX = needle.position.x;

            // TODO the Move To marker offset is unmeasured. The throw needle's is local (0, 9, 0),
            // so this one is probably Y only and needleChildX - needleX would be 0.
            var needleChildX = needleChild.position.x;

            var distance = needleChildX - needleX;
            Fsm.GetFsmFloat(SsdVars.Distance).Value = distance;

            SsdLog.LogDebug("retracting over " + distance);
            
            needle.gameObject.SetActive(true);
            
            stickNeedle.SetActive(false);
            
           
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(distance, 0f, 0f);
            
            damager.gameObject.SetActive(false);
        }
    }
}
