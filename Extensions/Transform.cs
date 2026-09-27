using UnityEngine;

namespace SilkSoarDash.Extensions
{
    public static class SsdTransform
    {
        private const float ForwardAngle = 90f;

        public static void PointForward(this Transform effect)
        {
            if (IsForward(effect))
            {
                return;
            }

            var local = effect.localPosition;

            effect.localPosition = new Vector3(-local.y, local.x, local.z);
            effect.TurnForward();
        }

        public static void PointUp(this Transform effect)
        {
            if (!IsForward(effect))
            {
                return;
            }

            var local = effect.localPosition;

            effect.localPosition = new Vector3(local.y, -local.x, local.z);
            effect.TurnUp();
        }

        public static void TurnForward(this Transform effect)
        {
            effect.localEulerAngles = new Vector3(0f, 0f, ForwardAngle);
        }

        public static void TurnUp(this Transform effect)
        {
            effect.localEulerAngles = Vector3.zero;
        }

        private static bool IsForward(Transform effect)
        {
            return Mathf.Approximately(effect.localEulerAngles.z, ForwardAngle);
        }
    }
}
