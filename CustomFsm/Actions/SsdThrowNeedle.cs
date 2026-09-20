using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedle : FsmStateAction
    {
        public override void OnEnter()
        {
            var thread = HeroController.instance.transform.Find("Effects/Super Jump Thread");

            if (thread != null && Fsm.GetFsmFloat(SsdVars.Distance).Value > SsdVars.ShortThrowThreshold)
            {
                thread.gameObject.SetActive(true);
            }
            
            Fsm.Event(SsdEvents.Cancelled);
            Finish();
        }
    }
}