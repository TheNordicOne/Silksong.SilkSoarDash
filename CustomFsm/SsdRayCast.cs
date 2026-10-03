using BepInEx.Logging;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm
{
    public static class SsdRayCast
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For(typeof(SsdRayCast));

        public static bool HitNoSuperJumpZone(Fsm fsm)
        {
            var hitPoint = fsm.GetFsmVector2(SsdVars.HitPoint).Value;
            if (NoSuperJumpCollider.IsInside(hitPoint))
            {
                return true;
            }

            var hitObject = fsm.GetFsmGameObject(SsdVars.HitObject).Value;
            return hitObject.GetComponent<NoSuperJumpCollider>();
        }

        public static void CastAndStore(Fsm fsm, Vector2 dir, FsmState state)
        {
            fsm.GetFsmFloat(SsdVars.Direction).Value = dir.x;
            var rc = BuildRayCaster(fsm, dir);

            rc.Init(state);

            fsm.GetFsmBool(SsdVars.IsGate).Value = false;

            rc.OnEnter();

            StoreHitAhead(fsm, dir);
            StoreSideGateAhead(fsm, dir);
        }

        private static SuperJumpRaycast BuildRayCaster(Fsm fsm, Vector2 dir)
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

                StoreDidHit = fsm.GetFsmBool(SsdVars.DidHit),
                StoreHitObject = fsm.GetFsmGameObject(SsdVars.HitObject),
                StoreHitPoint = fsm.GetFsmVector2(SsdVars.HitPoint),
                StoreDistance = fsm.GetFsmFloat(SsdVars.Distance),
                StoreIsTransitionGate = fsm.GetFsmBool(SsdVars.IsGate),
                StoreHitSpikes = fsm.GetFsmBool(SsdVars.HitSpikes)
            };
        }

        // SuperJumpRaycast skips triggers, so this measures terrain triggers it would miss
        private static void StoreHitAhead(Fsm fsm, Vector2 dir)
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

            // a miss leaves the SuperJumpRaycast result alone
            if (hitCount == 0)
            {
                return;
            }

            if (!IsNearestSoFar(fsm, hits[0].distance))
            {
                return;
            }

            StoreHit(fsm, hits[0]);
        }

        // SuperJumpRaycast only accepts a gate at the top of the room, so a sideways soar needs the side gates added
        private static void StoreSideGateAhead(Fsm fsm, Vector2 dir)
        {
            var origin = HeroController.instance.transform.position;
            var wantedSide = dir.x > 0f ? GatePosition.right : GatePosition.left;
            TransitionPoint nearestGate = null;
            var nearestDistance = SsdVars.NeedleRayDistance;

            foreach (var gate in TransitionPoint.TransitionPoints)
            {
                if (gate.GetGatePosition() != wantedSide)
                {
                    continue;
                }

                var bounds = gate.GetComponent<Collider2D>().bounds;
                if (origin.y < bounds.min.y || origin.y > bounds.max.y)
                {
                    continue;
                }

                var distance = dir.x > 0f ? bounds.min.x - origin.x : origin.x - bounds.max.x;
                if (distance < 0f || distance >= nearestDistance)
                {
                    continue;
                }

                nearestGate = gate;
                nearestDistance = distance;
            }

            if (nearestGate == null || !IsNearestSoFar(fsm, nearestDistance))
            {
                return;
            }

            SsdLog.Debug("gate {Gate} at={GateDistance} replaces hit={Hit} object={Object} at={Distance}", nearestGate.name, nearestDistance, fsm.GetFsmBool(SsdVars.DidHit).Value, fsm.GetFsmGameObject(SsdVars.HitObject).Value, fsm.GetFsmFloat(SsdVars.Distance).Value);

            var point = new Vector2(origin.x + nearestDistance * dir.x, origin.y);
            StoreHit(fsm, nearestGate.gameObject, point, nearestDistance);
            fsm.GetFsmBool(SsdVars.IsGate).Value = true;
        }

        private static bool IsNearestSoFar(Fsm fsm, float distance)
        {
            var hasHit = fsm.GetFsmBool(SsdVars.DidHit).Value;
            if (!hasHit)
            {
                return true;
            }

            var storedDistance = fsm.GetFsmFloat(SsdVars.Distance).Value;
            return distance < storedDistance;
        }

        private static void StoreHit(Fsm fsm, RaycastHit2D hit)
        {
            StoreHit(fsm, hit.collider.gameObject, hit.point, hit.distance);
        }

        private static void StoreHit(Fsm fsm, GameObject hitObject, Vector2 point, float distance)
        {
            fsm.GetFsmBool(SsdVars.DidHit).Value = true;
            fsm.GetFsmBool(SsdVars.IsGate).Value = false;
            fsm.GetFsmGameObject(SsdVars.HitObject).Value = hitObject;
            fsm.GetFsmVector2(SsdVars.HitPoint).Value = point;
            fsm.GetFsmFloat(SsdVars.Distance).Value = distance;
        }
    }
}