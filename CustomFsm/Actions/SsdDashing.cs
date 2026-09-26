using HutongGames.PlayMaker;
using UnityEngine;
using SilkSoarDash.Extensions;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashing : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        private MeshRenderer _threadLoop;
        
        public override void OnEnter()
        {
            // 1  start the flight loop
            //    - audio      hornet_flying_through_air_fast_loop
            //    - vibration  Sounds/Superjump Loop
            
            // 2  HeroController.AffectedByGravity(false)
            Hero.AffectedByGravity(false);
            
            // 3  Hero.Body.gravityScale = 0
            Hero.Body.gravityScale = 0;

            // 4  shake the camera
            //    - shake  Tiny Rumble

            var threadLoop = Hero.transform.Find(SsdObjects.ThreadLoop);
            _threadLoop = threadLoop == null ? null : threadLoop.GetComponent<MeshRenderer>();
        }

        public override void OnFixedUpdate()
        {
            // 5  velocity = (JumpSpeed * Direction, 0)
            Hero.ApplySsdVelocity(Fsm);
        }

        public override void OnUpdate()
        {
            // 6  wall hit ahead -> hit wall
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var wallVariable = direction > 0f ? SsdVars.WallHitRight : SsdVars.WallHitLeft;
            var hasHitWall = Fsm.GetFsmBool(wallVariable).Value;
            
            if (hasHitWall)
            {
                Fsm.Event(SsdEvents.HitWall);
                return;
            }

            // 7  speed along Direction <= 0.1 -> hit wall
            if (Hero.HasStopped(direction))
            {
                Fsm.Event(SsdEvents.HitWall);
                return;
            }
            
            // 8  raycast 10 ahead, toggle the thread loop renderer every frame
            ShowThreadLoop(direction);

            
            Finish();
        }

        private void ShowThreadLoop(float direction)
        {
            if (_threadLoop == null)
            {
                return;
            }

            var origin = Hero.transform.position;
            var ahead = new Vector2(direction, 0f);
            var hit = Physics2D.Raycast(origin, ahead, SsdVars.ThreadRayDistance, 1 << SsdVars.TerrainLayer);

            _threadLoop.enabled = hit.collider == null;
        }
    }
}
