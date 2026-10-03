using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdAirTargetCheck : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnUpdate()
        {
            if (!Fsm.GetFsmBool(SsdVars.AirTarget).Value)
            {
                return;
            }

            var needle = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var remaining = (needle.transform.position.x - Hero.transform.position.x) * direction;
            if (remaining > SsdVars.NeedleCatchDistance)
            {
                return;
            }

            Fsm.Event(SsdEvents.Catch);
        }
    }
}
