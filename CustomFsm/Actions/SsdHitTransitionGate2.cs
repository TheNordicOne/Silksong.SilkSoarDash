using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitTransitionGate2 : FsmStateAction
    {
        public override void OnEnter()
        {
            var needleStick = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            needleStick.SetActive(false);

            Finish();
        }
    }
}
