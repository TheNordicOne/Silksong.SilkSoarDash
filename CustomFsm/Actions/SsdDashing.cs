using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        
        public override void OnEnter()
        {
            // audio      hornet_flying_through_air_fast_loop
            // vibration  Sounds/Superjump Loop
            
            Hero.AffectedByGravity(false);

            Hero.Body.gravityScale = 0;
            
            // shake  Tiny Rumble
        }
    }
}
