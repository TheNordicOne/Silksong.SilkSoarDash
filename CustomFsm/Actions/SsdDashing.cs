using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        
        public override void OnEnter()
        {
            SsdEffects.StartLoop(SsdAudio.FlyingLoop);
            SsdEffects.StartLoopVibration();

            Hero.AffectedByGravity(false);

            Hero.Body.gravityScale = 0;
            
            SsdEffects.StartRumble();
        }

        public override void OnExit()
        {
            SsdEffects.StopLoop();
            SsdEffects.StopRumble();
        }
    }
}
