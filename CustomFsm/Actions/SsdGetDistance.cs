using BepInEx.Logging;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdGetDistance : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdGetDistance>();

        public override void OnEnter()
        {
            var dir = HeroController.instance.cState.facingRight ? Vector2.right : Vector2.left;
            Fsm.GetFsmFloat(SsdVars.Direction).Value = dir.x;

            var rc = BuildRayCaster(dir);

            rc.Init(State);
            Fsm.GetFsmFloat(SsdVars.Distance).Value = SsdVars.DefaultThrowDistance;
           
            StoreHitAhead(dir);
            
            rc.OnEnter();
            
            var dist = Fsm.GetFsmFloat(SsdVars.Distance).Value;
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(dist * dir.x, 0f, 0f);

            SsdLog.Debug("measured direction={Direction} hit={Hit} distance={Distance} object={Object} gate={Gate} spikes={Spikes}", dir.x, Fsm.GetFsmBool(SsdVars.DidHit).Value, dist, Fsm.GetFsmGameObject(SsdVars.HitObject).Value, Fsm.GetFsmBool(SsdVars.IsGate).Value, Fsm.GetFsmBool(SsdVars.HitSpikes).Value);

            Fsm.Event(SsdEvents.ThrowNeedle);
            Finish();
        }

        // SuperJumpRaycast skips triggers, so this measures terrain triggers it would miss
        private void StoreHitAhead(Vector2 dir)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = 1 << SsdVars.TerrainLayer
            };

            var hits = new RaycastHit2D[1];
            var origin = HeroController.instance.transform.position;
            var hitCount = Physics2D.Raycast(origin, dir, filter, hits, SsdVars.NeedleRayDistance);

            Fsm.GetFsmBool(SsdVars.DidHit).Value = hitCount > 0;

            if (hitCount == 0)
            {
                return;
            }

            Fsm.GetFsmGameObject(SsdVars.HitObject).Value = hits[0].collider.gameObject;
            Fsm.GetFsmVector2(SsdVars.HitPoint).Value = hits[0].point;
            Fsm.GetFsmFloat(SsdVars.Distance).Value = hits[0].distance;
        }

        private SuperJumpRaycast BuildRayCaster(Vector2 dir)
        {
            return new SuperJumpRaycast
            {
                Direction = new FsmVector2
                {
                    Value = dir
                },
                Space = Space.World,
                Distance = new FsmFloat
                {
                    Value = SsdVars.NeedleRayDistance
                },
                FromPosition = new FsmVector2
                {
                    Value = Vector2.zero
                },
                FromGameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = new FsmGameObject
                    {
                        Value = HeroController.instance.gameObject
                    }
                },

                StoreDidHit = Fsm.GetFsmBool(SsdVars.DidHit),
                StoreHitObject = Fsm.GetFsmGameObject(SsdVars.HitObject),
                StoreHitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint),
                StoreDistance = Fsm.GetFsmFloat(SsdVars.Distance),
                StoreIsTransitionGate = Fsm.GetFsmBool(SsdVars.IsGate),
                StoreHitSpikes = Fsm.GetFsmBool(SsdVars.HitSpikes),
            };
        }
    }
}