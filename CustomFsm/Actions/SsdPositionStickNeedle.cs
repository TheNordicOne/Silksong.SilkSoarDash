using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedle : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private const float DistanceOffset = 15f;

        public override void OnEnter()
        {
            // 1  read HeroX
            var heroX = Hero.transform.position.x;

            // 2  isDistant = StickNeedleX > HeroX + 15
            var hitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint).Value;
            var isDistant = hitPoint.x > (heroX + DistanceOffset);

            // 3  clip = distant clip if isDistant, near clip otherwise
            //    - audio  hornet_superjump_pt_5_needle_impact_2d_distant
            //    - audio  hornet_superjump_pt_5_needle_impact_2d
            var audioClip = isDistant 
                ? "hornet_superjump_pt_5_needle_impact_2d_distant" 
                : "hornet_superjump_pt_5_needle_impact_2d";

            // 4  play the clip
            
            // 5  PlayedThrowWait true -> finish here
            
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Throw Wait animation completes
            
            Finish();
        }
    }
}