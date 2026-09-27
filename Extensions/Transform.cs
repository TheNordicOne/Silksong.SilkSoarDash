using UnityEngine;

namespace SilkSoarDash.Extensions
{
    public static class SsdTransform
    {
        private const float ForwardAngle = 90f;

        public static void PointForward(this Transform effect)
        {
            if (Mathf.Approximately(effect.localEulerAngles.z, ForwardAngle))
            {
                return;
            }

            var local = effect.localPosition;

            effect.localPosition = new Vector3(-local.y, local.x, local.z);
            effect.localEulerAngles = new Vector3(0f, 0f, ForwardAngle);
        }

        public static void PointUp(this Transform effect)
        {
            if (!Mathf.Approximately(effect.localEulerAngles.z, ForwardAngle))
            {
                return;
            }

            var local = effect.localPosition;

            effect.localPosition = new Vector3(local.y, -local.x, local.z);
            effect.localEulerAngles = Vector3.zero;
        }
    }
}
