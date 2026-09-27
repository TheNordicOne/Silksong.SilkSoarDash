using BepInEx.Logging;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedlePre : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdPositionStickNeedlePre>();

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

            // store the stick needle's current parent
            Fsm.GetFsmGameObject(SsdVars.StickNeedleParent).Value = _needleStick.parent.gameObject;
            
            _needleStick.position = new Vector3(hitPoint.x, _needle.position.y, _needleStick.position.z);
            _needleStick.SetParent(null, true);
            _needleStick.PlayAnim(SsdAnims.NeedleWallHit);

            
            var isGate = Fsm.GetFsmBool(SsdVars.IsGate).Value;
            if (isGate)
            {
                Fsm.Event(SsdEvents.TransitionGate);
                return;
            }

            var isNeedleOffScreen = IsOutsideCamera();
            Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value = isNeedleOffScreen;
            
            if (isNeedleOffScreen)
            {
                Finish();
                return;
            }
            
            // effect  Nail Terrain Hit Effect

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
                    GameObject = new FsmGameObject
                    {
                        Value = _needleStick.gameObject
                    }
                },
                margin = new FsmFloat
                {
                    Value = 0f
                },
                outsideEvent = null,
                insideEvent = null,
                insideBool = new FsmBool(),
                outsideBool = storeIsOutside,
                everyFrame = false
            };
        }
    }
}