using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRegainControlToIdle : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdRegainControlToIdle>();

        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            var onWall = Hero.cState.superDashOnWall;

            Hero.SetCState(SsdCStates.SuperDashOnWall, false);
            Hero.SetCState(SsdCStates.SuperDashing, false);
            
            Hero.RegainControl();
            Hero.AffectedByGravity(true);

            // like the harpoon dash, a wall arrival hands over to wall sliding when the game allows it
            if (onWall && StartWallSlide())
            {
                SsdLog.Debug("regained into={Into}", "wall slide");
                Finish();
                return;
            }

            Hero.StartAnimationControlToIdle();
            SsdLog.Debug("regained into={Into} onWall={OnWall}", "idle", onWall);

            Finish();
        }

        private static bool StartWallSlide()
        {
            Hero.StartAnimationControl();
            return Hero.TryFsmCancelToWallSlide();
        }
    }
}
