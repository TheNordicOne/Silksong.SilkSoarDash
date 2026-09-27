using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRetractNeedleCancel : FsmStateAction
    {
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
            var needleChildX = needleChild.position.x;

            var distance = needleChildX - needleX;
            Fsm.GetFsmFloat(SsdVars.Distance).Value = distance;
            
            needle.gameObject.SetActive(true);
            
            stickNeedle.SetActive(false);
            
           
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(distance, 0f, 0f);
            
            damager.gameObject.SetActive(false);
        }
    }
}
