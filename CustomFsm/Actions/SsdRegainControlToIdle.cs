using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdRegainControlToIdle : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var onWall = SsdHeroState.OnWall;

            SsdHeroState.OnWall = false;
            SsdHeroState.Dashing = false;

            Hero.RegainControl();
            Hero.AffectedByGravity(true);

            // like the harpoon dash, a wall arrival hands over to wall sliding when the game allows it
            if (onWall && StartWallSlide())
            {
                Finish();
                return;
            }

            Hero.StartAnimationControlToIdle();

            Finish();
        }

        private static bool StartWallSlide()
        {
            Hero.StartAnimationControl();
            return Hero.TryFsmCancelToWallSlide();
        }
    }
}
