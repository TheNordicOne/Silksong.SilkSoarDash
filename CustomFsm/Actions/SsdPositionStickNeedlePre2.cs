using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedlePre2 : FsmStateAction
    {
        private Transform _needle;
        private Transform _needleStick;

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            _needle = SsdClones.ThrowNeedle;
            _needle.gameObject.SetActive(false);

            _needleStick = SsdClones.StickNeedle;
            _needleStick.gameObject.SetActive(true);

            Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value = _needleStick.gameObject;

            var didHit = Fsm.GetFsmBool(SsdVars.DidHit).Value;

            if (!didHit)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            var didHitSpikes = Fsm.GetFsmBool(SsdVars.HitSpikes).Value;

            if (didHitSpikes)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            var hitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint).Value;
            var isColliding = NoSuperJumpCollider.IsInside(hitPoint);

            if (isColliding)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            var hitObject = Fsm.GetFsmGameObject(SsdVars.HitObject).Value;
            var hasNoSuperJumpCollider = hitObject.GetComponent<NoSuperJumpCollider>();

            if (hasNoSuperJumpCollider)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            Fsm.GetFsmGameObject(SsdVars.StickNeedleParent).Value = _needleStick.parent.gameObject;

            // no needle was thrown in this room, so Hornet's height stands in for it
            _needleStick.position = new Vector3(hitPoint.x, Hero.transform.position.y, _needleStick.position.z);
            _needleStick.SetParent(null, true);
            _needleStick.PlayAnim(SsdAnims.NeedleWallHit);


            var isGate = Fsm.GetFsmBool(SsdVars.IsGate).Value;
            if (isGate)
            {
                Fsm.Event(SsdEvents.TransitionGate);
                return;
            }

            Finish();
        }
    }
}