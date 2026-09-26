using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  stop the charge loop audio
            //    - audio  Sounds/Superjump Loop
            // 2  play the antic clip
            //    - audio  hornet_superjump_pt_6_hornet_jump_antic
            // 3  play the antic animation
            //    - anim  Super Jump Jump Antic
        }

        public override void OnUpdate()
        {
            // 4  finish when the antic animation completes
            Finish();
        }
    }
}
