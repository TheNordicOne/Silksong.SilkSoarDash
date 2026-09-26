using HutongGames.PlayMaker;
using SilkSoarDash.Extensions;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        
        public override void OnEnter()
        {
            // audio      hornet_flying_through_air_fast_loop
            // vibration  Sounds/Superjump Loop
            
            Hero.AffectedByGravity(false);

            Hero.Body.gravityScale = 0;
            
            // shake  Tiny Rumble
        }

        public override void OnFixedUpdate()
        {
            Hero.ApplySsdVelocity(Fsm);
        }

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
