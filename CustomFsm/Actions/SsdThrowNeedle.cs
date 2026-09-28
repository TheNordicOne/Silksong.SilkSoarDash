using BepInEx.Logging;
using HutongGames.PlayMaker;
using UnityEngine;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowNeedle : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdThrowNeedle>();

        private Transform _needle;
        private Transform _damager;
        private float _dir;
        private float _startX;
        private Rigidbody2D _body;

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
            
            var travelled = Travelled();

            if (!ShouldFinish(travelled))
            {
                return;
            }

            SsdLog.Debug("needle done travelled={Travelled} target={Target}", travelled, Fsm.GetFsmFloat(SsdVars.Distance).Value);
            Fsm.Event(SsdEvents.Finished);
        }


        private void SetThreadEffect()
        {
            var threadEffect = SsdClones.Thread;
            if (threadEffect == null || !(Fsm.GetFsmFloat(SsdVars.Distance).Value > SsdVars.ShortThrowThreshold))
            {
                return;
            }
            threadEffect.gameObject.SetActive(true);
        }

        private static void PreThrowEffects()
        {

            SsdEffects.PlayVoice(SsdAudio.AttackHeavyVoice);
            SsdEffects.PlayOneShot(SsdAudio.Throw, SsdAudio.FlatPitch, SsdAudio.FlatPitch);
            SsdEffects.Vibrate(SsdVibration.NeedleThrow);

            var chargedEffect = SsdClones.ChargedEffect;
            if (chargedEffect != null)
            {
                chargedEffect.gameObject.SetActive(false);
            }
        }

        private bool TryThrowNeedle()
        {
            _dir = Fsm.GetFsmFloat(SsdVars.Direction).Value;

            _needle = SsdClones.ThrowNeedle;

            if (_needle == null)
            {
                SsdLog.Warning("needle clone missing, cancelling");
                return false;
            }

            _damager = _needle.Find(SsdObjects.NeedleDamagerChild);
            _needle.position = Hero.transform.position + new Vector3(SsdVars.NeedleStartForward * _dir, SsdVars.NeedleStartHeight, 0f);
            _needle.gameObject.SetActive(true);
            PutTailAhead();
            _body = _needle.GetComponent<Rigidbody2D>();
            _body.position = _needle.position;
            _body.linearVelocity = new Vector2(SsdVars.NeedleThrowSpeed * _dir, 0f);
            _startX = _needle.position.x;

            return true;
        }


        // the sprite trails far behind its pivot, so placing the pivot ahead of Hornet still draws the needle through her
        private void PutTailAhead()
        {
            var sprites = _needle.GetComponentsInChildren<Renderer>();
            if (sprites.Length == 0)
            {
                SsdLog.Warning("needle has no renderer, start not adjusted");
                return;
            }

            var bounds = sprites[0].bounds;
            foreach (var sprite in sprites)
            {
                bounds.Encapsulate(sprite.bounds);
            }

            var tail = _dir > 0f ? bounds.min.x : bounds.max.x;
            var wantedTail = Hero.transform.position.x + SsdVars.NeedleStartForward * _dir;
            _needle.position += new Vector3(wantedTail - tail, 0f, 0f);
        }

        private void SetDamager()
        {
            var needleDistanceFromHornet = Mathf.Abs(_needle.position.x - Hero.transform.position.x);

            if (_damager != null)
            {
                _damager.gameObject.SetActive(needleDistanceFromHornet <= SsdVars.NeedleDamagerRange);
            }
        }

        private float Travelled()
        {
            return (_needle.position.x - _startX) * _dir;
        }

        private bool ShouldFinish(float travelled)
        {
            var backToBeginning = travelled < SsdVars.NeedleReturnedDistance;
            var landed = travelled > 0f && Mathf.Abs(_body.linearVelocityX) < SsdVars.StoppedSpeed;

            return backToBeginning || landed;
        }
    }
}