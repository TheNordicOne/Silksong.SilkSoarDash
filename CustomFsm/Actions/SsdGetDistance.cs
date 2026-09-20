using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdGetDistance : FsmStateAction
    {
        public override void OnEnter()
        {
            var rc = BuildRayCaster();

            rc.Init(State);
            Fsm.GetFsmFloat(SsdVars.Distance).Value = SsdVars.DefaultThrowDistance;
            rc.OnEnter();

            Fsm.Event(SsdEvents.ThrowNeedle);
            Finish();
        }

        private SuperJumpRaycast BuildRayCaster()
        {
            var dir = HeroController.instance.cState.facingRight ? Vector2.right : Vector2.left;

            return new SuperJumpRaycast
            {
                Direction = new FsmVector2
                {
                    Value = dir
                },
                Space = Space.World,
                Distance = new FsmFloat { Value = 350f },
                FromPosition = new FsmVector2 { Value = Vector2.zero },
                FromGameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = new FsmGameObject { Value = HeroController.instance.gameObject }
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