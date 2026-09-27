using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWallCheck : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdHitWallCheck>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnUpdate()
        {
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var wallVariable = direction > 0f ? SsdVars.WallHitRight : SsdVars.WallHitLeft;
            var hasHitWall = Fsm.GetFsmBool(wallVariable).Value;
            
            var hasStopped = Hero.HasStopped(direction);

            if (hasHitWall || hasStopped)
            {
                SsdLog.Debug("stopping wall={Wall} stopped={Stopped} direction={Direction}", hasHitWall, hasStopped, direction);
                Fsm.Event(SsdEvents.HitWall);
            }
        }
    }
}