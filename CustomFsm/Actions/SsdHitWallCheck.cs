using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWallCheck : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnUpdate()
        {
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var wallVariable = direction > 0f ? SsdVars.WallHitRight : SsdVars.WallHitLeft;
            var hasHitWall = Fsm.GetFsmBool(wallVariable).Value;
            
            if (hasHitWall || Hero.HasStopped(direction))
            {
                Fsm.Event(SsdEvents.HitWall);
            }
        }
    }
}