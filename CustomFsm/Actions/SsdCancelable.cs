using HutongGames.PlayMaker;
using SilkSoarDash.Extensions;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelable : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        
        public override void OnEnter()
        {
            // audio      Sounds/Superjump Loop
            // vibration  Sounds/Superjump Loop

            // QueuedCancel true -> cancel

            // shake  Tiny Rumble
        }

        public override void OnFixedUpdate()
        {
            Hero.ApplySsdVelocity(Fsm);
        }

        public override void OnUpdate()
        {
            // jump pressed -> cancel
            // attack pressed -> cancel
            // superdash pressed -> cancel

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
