using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // stop the charge loop audio
            //    - audio  Sounds/Superjump Loop
            // play the antic clip
            //    - audio  hornet_superjump_pt_6_hornet_jump_antic
            // play the antic animation
            //    - anim  Super Jump Jump Antic
        }

        public override void OnUpdate()
        {
            // finish when the antic animation completes
            Finish();
        }
    }
}
