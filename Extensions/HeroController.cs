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

        // the soar clip flies upward, so Hornet is turned toward the dash, hitbox included, and plays it sideways
        // the mirror scale is applied before the rotation, so the turn flips with the facing
        public static void EnterDashPose(this HeroController hero, float direction)
        {
            hero.TurnAboutBody(-SsdVars.DashPoseAngle * direction);
            hero.PlayAnim(SsdAnims.Loop);
        }

        public static void ExitDashPose(this HeroController hero)
        {
            hero.TurnAboutBody(0f);
        }

        // the transform pivot sits low on her, so a plain rotation would swing the hitbox into the floor
        private static void TurnAboutBody(this HeroController hero, float angle)
        {
            var collider = hero.GetComponent<Collider2D>();
            var centre = collider.bounds.center;
            hero.transform.localEulerAngles = new Vector3(0f, 0f, angle);
            Physics2D.SyncTransforms();
            hero.transform.position += centre - collider.bounds.center;
            Physics2D.SyncTransforms();
        }

        public static void ExitDashPoseAtWall(this HeroController hero, float direction)
        {
            var edge = hero.LeadingEdge(direction);
            hero.ExitDashPose();
            hero.transform.position += new Vector3(edge - hero.LeadingEdge(direction), 0f, 0f);
            Physics2D.SyncTransforms();
        }

        private static float LeadingEdge(this HeroController hero, float direction)
        {
            var bounds = hero.GetComponent<Collider2D>().bounds;
            return direction > 0f ? bounds.max.x : bounds.min.x;
        }

        public static float AnimSeconds(this HeroController hero, string clip)
        {
            var animationClip = hero.GetComponent<tk2dSpriteAnimator>().GetClipByName(clip);
            return animationClip == null ? 0f : animationClip.frames.Length / animationClip.fps;
        }

        private static float GetForwardSpeed(this HeroController hero, float direction)
        {
            return hero.Body.linearVelocity.x * direction;
        }

    }
}