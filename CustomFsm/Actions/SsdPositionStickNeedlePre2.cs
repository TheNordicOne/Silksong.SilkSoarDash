using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedlePre2 : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdPositionStickNeedlePre2>();

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

            Fsm.GetFsmBool(SsdVars.AirTarget).Value = false;

            var didHit = Fsm.GetFsmBool(SsdVars.DidHit).Value;

            if (!didHit)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            var hitPoint = Fsm.GetFsmVector2(SsdVars.HitPoint).Value;

            // the walls behind this room are unknown, so an unusable obstacle gets a needle in the air short of it
            if (IsUnusable(hitPoint))
            {
                PlaceAirNeedle(hitPoint);
                return;
            }

            PlaceNeedle(hitPoint.x, SsdAnims.NeedleWallHit);

            var isGate = Fsm.GetFsmBool(SsdVars.IsGate).Value;
            if (isGate)
            {
                Fsm.Event(SsdEvents.TransitionGate);
                return;
            }

            Finish();
        }

        private bool IsUnusable(Vector2 hitPoint)
        {
            if (Fsm.GetFsmBool(SsdVars.HitSpikes).Value)
            {
                return true;
            }

            if (NoSuperJumpCollider.IsInside(hitPoint))
            {
                return true;
            }

            var hitObject = Fsm.GetFsmGameObject(SsdVars.HitObject).Value;
            return hitObject.GetComponent<NoSuperJumpCollider>();
        }

        private void PlaceAirNeedle(Vector2 hitPoint)
        {
            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var needleX = hitPoint.x - SsdVars.AirNeedleGap * direction;
            if ((needleX - Hero.transform.position.x) * direction <= SsdVars.NeedleCatchDistance)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SsdLog.Debug("air needle at={X} obstacle={Obstacle} hero={Hero}", needleX, hitPoint.x, Hero.transform.position.x);

            Fsm.GetFsmBool(SsdVars.AirTarget).Value = true;
            PlaceNeedle(needleX, SsdAnims.NeedleAir);

            Finish();
        }

        // no needle was thrown in this room, so Hornet's height stands in for it
        private void PlaceNeedle(float x, string clip)
        {
            Fsm.GetFsmGameObject(SsdVars.StickNeedleParent).Value = _needleStick.parent.gameObject;

            _needleStick.position = new Vector3(x, Hero.transform.position.y, _needleStick.position.z);
            _needleStick.SetParent(null, true);
            _needleStick.PlayAnim(clip);
        }
    }
}