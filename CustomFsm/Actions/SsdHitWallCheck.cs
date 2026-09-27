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
            // the side collision check never saw the wall, stopping dead is what tells us she arrived
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            if (!Hero.HasStopped(direction))
            {
                return;
            }

            SsdLog.Debug("stopping direction={Direction}", direction);
            Fsm.Event(SsdEvents.HitWall);
        }
    }
}