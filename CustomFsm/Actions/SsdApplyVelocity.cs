using HutongGames.PlayMaker;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdApplyVelocity : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnFixedUpdate()
        {
            Hero.ApplySsdVelocity(Fsm);
        }
    }
}