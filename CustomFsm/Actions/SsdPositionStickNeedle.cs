using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedle : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {

            var heroX = Hero.transform.position.x;
            
            var hitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint).Value;
            var isDistant = hitPoint.x > heroX + SsdVars.DistantImpactRange;

            var audioClip = isDistant ? SsdAudio.NeedleImpactDistant : SsdAudio.NeedleImpact;
            SsdEffects.PlayOneShot2D(audioClip, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
            // PlayedThrowWait true -> finish here
        }

        public override void OnUpdate()
        {
            // finish when the Super Jump Throw Wait animation completes
            
            Finish();
        }
    }
}