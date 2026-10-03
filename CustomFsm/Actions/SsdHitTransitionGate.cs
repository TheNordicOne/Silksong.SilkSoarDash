using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitTransitionGate : FsmStateAction
    {
        public override void OnEnter()
        {
            var needleStick = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            needleStick.SetActive(false);

            SsdEffects.PlayOneShot2D(SsdAudio.NeedleImpactDistant, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
        }

        public override void OnUpdate()
        {
            Finish();
        }
    }
}
