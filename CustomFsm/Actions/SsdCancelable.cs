using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancelable : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        
        public override void OnEnter()
        {
            // audio      Sounds/Superjump Loop
            // vibration  Sounds/Superjump Loop

            // QueuedCancel true -> cancel

            // shake  Tiny Rumble
        }

        public override void OnUpdate()
        {
            // jump pressed -> cancel
            // attack pressed -> cancel
            // superdash pressed -> cancel
            
        }
    }
}
