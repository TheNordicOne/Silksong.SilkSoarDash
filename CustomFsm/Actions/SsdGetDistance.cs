using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdGetDistance : FsmStateAction
    {
        public override void OnEnter()
        {
            var dir = HeroController.instance.cState.facingRight ? Vector2.right : Vector2.left;
            Fsm.GetFsmFloat(SsdVars.Direction).Value = dir.x;

            var rc = BuildRayCaster(dir);

            rc.Init(State);
            // TODO DefaultThrowDistance is the throw needle's Move To Y offset. No horizontal
            // equivalent exists in the game, so this fallback distance needs deciding.
            Fsm.GetFsmFloat(SsdVars.Distance).Value = SsdVars.DefaultThrowDistance;
            rc.OnEnter();
            
            var dist = Fsm.GetFsmFloat(SsdVars.Distance).Value;
            Fsm.GetFsmVector3(SsdVars.MoveBy).Value = new Vector3(dist * dir.x, 0f, 0f);

            Fsm.Event(SsdEvents.ThrowNeedle);
            Finish();
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