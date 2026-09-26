using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedlePre : FsmStateAction
    {
        private Transform _needle;
        private Transform _needleStick;

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            _needle = Hero.transform.Find(SsdObjects.ThrowNeedle);
            // 1  Special Attacks/Super Jump Needle Throw OFF
            _needle.gameObject.SetActive(false);

            // 2  Special Attacks/Super Jump Needle Stick ON
            _needleStick = Hero.transform.Find(SsdObjects.StickNeedle);
            _needleStick.gameObject.SetActive(true);
            Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value = _needleStick.gameObject;

            // 3  DidHit false -> cancel
            var didHit = Fsm.GetFsmBool(SsdVars.DidHit).Value;

            if (!didHit)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            // 4  NoSuperJumpCollider.IsInside(HitPoint) true -> cancel
            var hitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint).Value;
            var isColliding = NoSuperJumpCollider.IsInside(hitPoint);

            if (isColliding)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            // 5  HitObject has a NoSuperJumpCollider -> cancel
            var hitObject = Fsm.GetFsmGameObject(SsdVars.HitObject).Value;
            var hasNoSuperJumpCollider = hitObject.GetComponent<NoSuperJumpCollider>();

            if (hasNoSuperJumpCollider)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            // 6  store the stick needle's current parent
            Fsm.GetFsmGameObject(SsdVars.StickNeedleParent).Value = _needleStick.parent.gameObject;

            // 7  unparent the stick needle, keep its world position
            _needleStick.SetParent(null, true);

            // 8  read the throw needle Y
            var needleY = _needle.position.y;

            // 9  move the stick needle to HitPoint
            _needleStick.position = new Vector3(hitPoint.x, hitPoint.y, _needleStick.position.z);

            // 10 read the stick needle Y
            var stickY = _needleStick.position.y;

            // 11 offset = throw needle Y - stick needle Y
            var offsetY = needleY - stickY;

            // 12 translate the stick needle by offset on Y
            _needleStick.Translate(0f, offsetY, 0f, Space.World);

            // 13 IsGate true -> transition gate
            var isGate = Fsm.GetFsmBool(SsdVars.IsGate).Value;
            if (isGate)
            {
                Fsm.Event(SsdEvents.TransitionGate);
                return;
            }

            // 14 stick needle off camera -> finish here
            var isNeedleOffScreen = IsOutsideCamera();
            Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value = isNeedleOffScreen;
            
            if (isNeedleOffScreen)
            {
                Finish();
                return;
            }

            // 15 spawn the terrain hit effect
            //    - effect  Nail Terrain Hit Effect

            Finish();
        }

        private bool IsOutsideCamera()
        {
            var isOutsideCamera = new FsmBool();
            var cameraCheck = BuildCameraCheck(isOutsideCamera);

            cameraCheck.Init(State);
            cameraCheck.OnEnter();
            return isOutsideCamera.Value;
        }

        private CheckOutOfCamera BuildCameraCheck(FsmBool storeIsOutside)
        {
            return new CheckOutOfCamera
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = new FsmGameObject { Value = _needleStick.gameObject }
                },
                margin = new FsmFloat { Value = 0f },
                outsideEvent = null,
                insideEvent = null,
                insideBool = new FsmBool(),
                outsideBool = storeIsOutside,
                everyFrame = false
            };
        }
    }
}