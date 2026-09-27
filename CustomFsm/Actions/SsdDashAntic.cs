using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        
        public override void OnEnter()
        {
            // stop the charge loop audio
            //    - audio  Sounds/Superjump Loop
            // play the antic clip
            //    - audio  hornet_superjump_pt_6_hornet_jump_antic

            Hero.PlayAnim(SsdAnims.JumpAntic);
        }

        public override void OnUpdate()
        {
            if (Hero.IsAnimPlaying(SsdAnims.JumpAntic))
            {
                return;
            }

            Finish();
        }
    }
}
