using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  start the flight loop
            //    - audio      hornet_flying_through_air_fast_loop
            //    - vibration  Superjump Loop
            // 2  start the cancelableTime timer
            // 3  HeroController.AffectedByGravity(false)
            // 4  gravityScale = 0
            // 5  show the thread loop after 0.1
            //    - effect  Super Jump Thread Loop
            // 6  shake the camera
            //    - shake  Tiny Rumble
        }

        public override void OnUpdate()
        {
            // 7  velocity = 33 along Direction
            // 8  wall hit ahead -> hit wall
            // 9  speed along Direction <= 0.1 -> hit wall
            // 10 raycast 10 ahead, toggle the thread loop renderer every frame
            // 11 cancelableTime elapsed -> cancelable
            
            Finish();
        }
    }
}
