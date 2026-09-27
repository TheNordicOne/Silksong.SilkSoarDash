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
            
            // vanilla plays Super Jump Loop Cancel and sets an updraft exit, both are for an upward soar
            // the game's own fall animation takes over once control is back
            // vanilla leaves this state by the clip's complete event, not by finishing its actions
            Fsm.Event(SsdEvents.Finished);
        }
    }
}
