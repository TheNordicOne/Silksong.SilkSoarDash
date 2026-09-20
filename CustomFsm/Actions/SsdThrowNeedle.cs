using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedle : FsmStateAction
    {
        private Transform _needle;
        private Transform _damager;
        private float _dir;
        private float _startX;
        private float _elapsed;

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
            _elapsed += Time.deltaTime;

            SetDamager();
            
            if (ShouldFinish())
            {
                Finish();
            }
        }


        private void SetThreadEffect()
        {
            var threadEffect = Hero.transform.Find("Effects/Super Jump Thread");
            if (threadEffect != null && Fsm.GetFsmFloat(SsdVars.Distance).Value > SsdVars.ShortThrowThreshold)
            {
                threadEffect.gameObject.SetActive(true);
            }
        }

        private static void PreThrowEffects()
        {
            // Presentation
            // - anim       Super Jump Throw Wait
            // - audio      Attack Heavy Hornet Voice
            // - audio      hornet_superjump_pt_4_throw
            // - vibration  hornet_need_throw_superjump

            var chargedEffect = Hero.transform.Find("Effects/Super Jump Charged");
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(false);
            }
        }

        private bool TryThrowNeedle()
        {
            _dir = Fsm.GetFsmFloat(SsdVars.Direction).Value;

            _needle = Hero.transform.Find("Special Attacks/Super Jump Needle Throw");

            if (_needle == null)
            {
                return false;
            }

            _damager = _needle.Find("Damager");
            _needle.localPosition = new Vector3(0f, SsdVars.NeedleStartHeight, 0f);
            _needle.gameObject.SetActive(true);
            _needle.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(150f * _dir, 0f);
            _startX = _needle.position.x;

            _elapsed = 0f;
            return true;
        }


        private void SetDamager()
        {
            var needleDistanceFromHornet = Mathf.Abs(_needle.position.x - Hero.transform.position.x);

            if (_damager != null)
            {
                _damager.gameObject.SetActive(needleDistanceFromHornet <= 30f);
            }
        }

        private bool ShouldFinish()
        {
            var travelled = (_needle.position.x - _startX) * _dir;
            var needleLanded = travelled >= Fsm.GetFsmFloat(SsdVars.Distance).Value;
            var backToBeginning = travelled < -0.1f;
            var timeout = _elapsed > 0.8f;

            return timeout || needleLanded || backToBeginning;
        }
    }
}