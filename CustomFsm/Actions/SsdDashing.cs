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
            // audio      hornet_flying_through_air_fast_loop
            // vibration  Sounds/Superjump Loop
            
            Hero.AffectedByGravity(false);

            Hero.Body.gravityScale = 0;
            
            // shake  Tiny Rumble

            var threadLoop = Hero.transform.Find(SsdObjects.ThreadLoop);
            _threadLoop = threadLoop == null ? null : threadLoop.GetComponent<MeshRenderer>();
        }

        public override void OnFixedUpdate()
        {
            Hero.ApplySsdVelocity(Fsm);
        }

        public override void OnUpdate()
        {
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var wallVariable = direction > 0f ? SsdVars.WallHitRight : SsdVars.WallHitLeft;
            var hasHitWall = Fsm.GetFsmBool(wallVariable).Value;
            
            if (hasHitWall || Hero.HasStopped(direction))
            {
                Fsm.Event(SsdEvents.HitWall);
                return;
            }
            
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
