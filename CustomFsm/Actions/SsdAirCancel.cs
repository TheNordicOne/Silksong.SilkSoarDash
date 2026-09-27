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

            // audio   Grunt Hornet Voice
            
            var needle = SsdClones.RetractNeedle;
            needle.gameObject.SetActive(false);
            needle.localPosition = new Vector3(0,SsdVars.NeedleStartHeight,0);
 

            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            Hero.SetCState(SsdCStates.SuperDashing, false);

            SsdShake.Send(SsdCamera.EnemyKillShake);
            
            Hero.SetStartWithUpdraftExit();

            Hero.PlayAnim(SsdAnims.LoopCancel);
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.LoopCancel))
            {
                return;
            }

            Finish();
        }
    }
}
