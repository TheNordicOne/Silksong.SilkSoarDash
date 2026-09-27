using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;


namespace SilkSoarDash.Extensions
{
    public static class SsdHeroController
    {
        public static void ApplySsdVelocity(this HeroController hero, Fsm fsm)
        {
            var jumpSpeed = fsm.GetFsmFloat(SsdVars.JumpSpeed).Value;
            var direction = fsm.GetFsmFloat(SsdVars.Direction).Value;
            hero.Body.linearVelocity = new Vector2(jumpSpeed * direction, 0f);
        }
        public static bool HasStopped(this HeroController hero, float direction)
        {
            return hero.GetForwardSpeed(direction) <= SsdVars.StoppedSpeed;
        }
        
        public static bool IsFalling(this HeroController hero)
        {
            return hero.Body.linearVelocityY < SsdVars.FallDetectionSpeed;
        }

        public static void PlayAnim(this HeroController hero, string clip)
        {
            var animator = hero.GetComponent<tk2dSpriteAnimator>();
            if (animator != null)
            {
                animator.Play(clip);
            }
        }

        public static bool IsAnimPlaying(this HeroController hero, string clip)
        {
            var animator = hero.GetComponent<tk2dSpriteAnimator>();
            return animator != null && animator.IsPlaying(clip);
        }

        private static float GetForwardSpeed(this HeroController hero, float direction)
        {
            return hero.Body.linearVelocity.x * direction;
        }

    }
}