using HutongGames.PlayMaker;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdApplyVelocity : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        // PlayMaker only calls OnFixedUpdate on an FSM that has an action asking for it
        public override void OnPreprocess()
        {
            Fsm.HandleFixedUpdate = true;
        }

        public override void OnFixedUpdate()
        {
            Hero.ApplySsdVelocity(Fsm);
        }
    }
}