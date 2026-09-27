using BepInEx.Logging;
using HutongGames.PlayMaker;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedle : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdThrowNeedle>();

        private Transform _needle;
        private Transform _damager;
        private float _dir;
        private float _startX;

        private static HeroController Hero => HeroController.instance;


        public override void OnEnter()
        {
            if (Hero == null)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            SetThreadEffect();

            PreThrowEffects();

            var needleThrown = TryThrowNeedle();
            if (needleThrown)
            {
                return;
            }

            Fsm.Event(SsdEvents.Cancelled);
            Finish();
        }

        public override void OnUpdate()
        {
            SetDamager();
            
            if (!ShouldFinish())
            {
                return;
            }

            SsdLog.LogDebug("landed at " + Mathf.Abs(_needle.position.x - _startX));
            Fsm.Event(SsdEvents.Finished);
        }


        private void SetThreadEffect()
        {
            var threadEffect = Hero.transform.Find(SsdObjects.Thread);
            if (threadEffect != null && Fsm.GetFsmFloat(SsdVars.Distance).Value > SsdVars.ShortThrowThreshold)
            {
                threadEffect.gameObject.SetActive(true);
            }
        }

        private static void PreThrowEffects()
        {
            // - anim       Super Jump Throw Wait
            // - audio      Attack Heavy Hornet Voice
            // - audio      hornet_superjump_pt_4_throw
            // - vibration  hornet_need_throw_superjump

            var chargedEffect = Hero.transform.Find(SsdObjects.ChargedEffect);
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(false);
            }
        }

        private bool TryThrowNeedle()
        {
            _dir = Fsm.GetFsmFloat(SsdVars.Direction).Value;

            _needle = Hero.transform.Find(SsdObjects.ThrowNeedle);

            if (_needle == null)
            {
                SsdLog.LogWarning(SsdObjects.ThrowNeedle + " not found");
                return false;
            }

            _damager = _needle.Find(SsdObjects.NeedleDamagerChild);
            _needle.localPosition = new Vector3(0f, SsdVars.NeedleStartHeight, 0f);
            _needle.gameObject.SetActive(true);
            _needle.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(SsdVars.NeedleThrowSpeed * _dir, 0f);
            _startX = _needle.position.x;

            return true;
        }


        private void SetDamager()
        {
            var needleDistanceFromHornet = Mathf.Abs(_needle.position.x - Hero.transform.position.x);

            if (_damager != null)
            {
                _damager.gameObject.SetActive(needleDistanceFromHornet <= SsdVars.NeedleDamagerRange);
            }
        }

        private bool ShouldFinish()
        {
            var travelled = (_needle.position.x - _startX) * _dir;
            var needleLanded = travelled >= Fsm.GetFsmFloat(SsdVars.Distance).Value;
            var backToBeginning = travelled < SsdVars.NeedleReturnedDistance;

            return needleLanded || backToBeginning;
        }
    }
}