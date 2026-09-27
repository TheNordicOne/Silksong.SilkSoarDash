using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace SilkSoarDash.Extensions
{
    internal static class SsdTransform
    {
        private const float ForwardAngle = 90f;

        public static void PointForward(this Transform effect)
        {
            var local = effect.localPosition;

            effect.localPosition = new Vector3(-local.y, local.x, local.z);
            effect.TurnForward();
        }

        // for an object that left the hero, whose mirrored rotation does not survive unparenting
        public static void TurnForwardInWorld(this Transform effect, float direction)
        {
            effect.rotation = Quaternion.Euler(0f, 0f, direction > 0f ? -ForwardAngle : ForwardAngle);
        }

        public static void TurnForward(this Transform effect)
        {
            effect.localEulerAngles = new Vector3(0f, 0f, ForwardAngle);
        }

        public static void SetAlpha(this Transform effect, float alpha)
        {
            var group = effect.GetComponent<NestedFadeGroupBase>();
            if (group != null)
            {
                group.AlphaSelf = alpha;
            }
        }

        public static void FadeTo(this Transform effect, float alpha, float fadeTime)
        {
            var group = effect.GetComponent<NestedFadeGroupBase>();
            if (group != null)
            {
                group.FadeTo(alpha, fadeTime);
            }
        }

        public static void PlayAnim(this Transform effect, string clip)
        {
            var animator = effect.GetComponent<tk2dSpriteAnimator>();
            if (animator != null)
            {
                animator.Play(clip);
            }
        }
    }
}
