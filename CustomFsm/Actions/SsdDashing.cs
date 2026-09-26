using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  start the flight loop
            //    - audio      hornet_flying_through_air_fast_loop
            //    - vibration  Sounds/Superjump Loop
            
            // 2  HeroController.AffectedByGravity(false)
            // 3  gravityScale = 0
            // 4  show the thread loop after 0.1
            //    - effect  Effects/Super Jump Thread Loop
            // 5  shake the camera
            //    - shake  Tiny Rumble
        }

        public override void OnUpdate()
        {
            // 6  velocity = (JumpSpeed * Direction, 0)
            // 7  wall hit ahead -> hit wall
            // 8  speed along Direction <= 0.1 -> hit wall
            // 9  raycast 10 ahead, toggle the thread loop renderer every frame
            
            Finish();
        }
    }
}
