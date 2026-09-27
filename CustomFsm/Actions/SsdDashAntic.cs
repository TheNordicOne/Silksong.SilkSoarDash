using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        public override void OnEnter()
        {
            // stop the charge loop audio
            //    - audio  Sounds/Superjump Loop
            //    - audio  hornet_superjump_pt_6_hornet_jump_antic

            // the harpoon clips go straight from Harpoon Throw to Harpoon Dash, so there is no jump antic to play
            Finish();
        }
    }
}
