using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirCancel : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            // effect  Effects/Super Jump Catch Effect
            // audio   Grunt Hornet Voice
            
            var needle = SsdClones.RetractNeedle;
            needle.gameObject.SetActive(false);
            needle.localPosition = new Vector3(0,SsdVars.NeedleStartHeight,0);
 

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            Hero.SetCState(SsdCStates.SuperDashing, false);

            // event  EnemyKillShake
            
            Hero.SetStartWithUpdraftExit();

            // anim  Super Jump Loop Cancel
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Loop Cancel animation completes
            Finish();
        }
    }
}
