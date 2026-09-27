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

        public static void TurnForward(this Transform effect)
        {
            effect.localEulerAngles = new Vector3(0f, 0f, ForwardAngle);
        }
    }
}
