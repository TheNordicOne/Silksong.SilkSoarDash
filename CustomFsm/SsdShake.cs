using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm
{
    // Rumble is a flag on the camera's own CameraShake FSM, shakes are events sent to it.
    public static class SsdShake
    {
        private static GameObject CameraParent
        {
            get
            {
                var cameras = GameCameras.instance;
                return cameras == null ? null : cameras.cameraParent.gameObject;
            }
        }

        public static void SetFocus(bool rumbling)
        {
            SetShakeBool(SsdCamera.RumblingFocus, rumbling);
        }

        public static void SetFocus2(bool rumbling)
        {
            SetShakeBool(SsdCamera.RumblingFocus2, rumbling);
        }

        public static void Send(string shakeEvent)
        {
            var cameraParent = CameraParent;
            if (cameraParent != null)
            {
                FSMUtility.SendEventToGameObject(cameraParent, shakeEvent);
            }
        }

        private static void SetShakeBool(string variable, bool value)
        {
            var cameraParent = CameraParent;
            if (cameraParent == null)
            {
                return;
            }

            var shake = FSMUtility.LocateFSM(cameraParent, SsdCamera.ShakeFsm);
            if (shake != null)
            {
                FSMUtility.SetBool(shake, variable, value);
            }
        }
    }
}
