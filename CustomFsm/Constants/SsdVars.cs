namespace SilkSoarDash.CustomFsm.Constants
{
    public static class SsdVars
    {
        public const string Distance = "Distance";
        public const string HitPoint = "HitPoint";
        public const string HitObject = "HitObject";
        public const string DidHit = "DidHit";
        public const string IsGate = "IsGate";
        public const string HitSpikes = "HitSpikes";
        public const string MoveBy = "MoveBy";
        public const string Direction = "Direction";
        public const string StickNeedle = "StickNeedle";
        public const string StickNeedleParent = "StickNeedleParent";
        public const string NeedleOffScreen = "NeedleOffScreen";
        public const string QueuedCancel = "QueuedCancel";
        public const string CancelableTime = "CancelableTime";
        public const string JumpSpeed = "JumpSpeed";
        public const string ChargeTime = "ChargeTime";
        public const string ThrowWaitTime = "ThrowWaitTime";
        public const string DidAddUsingSilk = "DidAddUsingSilk";
        public const string DidStartFlash = "DidStartFlash";
        public const string StartedRumblingFocus = "StartedRumblingFocus";
        public const string StartedRumblingFocus2 = "StartedRumblingFocus2";
        public const string AirTarget = "AirTarget";

        public const int DefaultThrowDistance = 9;
        public const float DefaultJumpSpeed = 33f;
        public const float DefaultChargeTime = 0.8f;
        public const float ThrowWaitTimeOffScreen = 0.5f;
        public const float NeedleThrowTimeout = 0.8f;
        public const float ThreadLoopDelay = 0.1f;
        public const float ChargingFaderFadeTime = 0.1f;
        public const float ThreadRayDistance = 10f;
        public const int TerrainLayer = 8;
        public const float DefaultCancelableTime = 0.2f;
        public const float QueuedCancelableTime = 0.05f;
        public const float StoppedSpeed = 0.1f;
        public const float CatchFallSpeed = 5f;
        public const float KickUpHeight = 0.3f;
        public const float DashPoseAngle = 90f;
        public const float EntryGateClearance = 0.05f;
        public const float FallDetectionSpeed = -0.1f;

        public const float NeedleStartHeight = 0f;
        public const float NeedleStartForward = 1.5f;
        public const int ShortThrowThreshold = 12;
        public const float NeedleThrowSpeed = 150f;
        public const float RetractNeedleSpeed = 150f;
        public const float RetractNeedleDistance = 11.7f;
        public const float CancelMomentumDecay = 1.2f;
        public const float ChargeDecelerationX = 0.9f;
        public const float ChargeDecelerationY = 0f;
        public const float NeedleDamagerRange = 30f;
        public const float NeedleReturnedDistance = -0.1f;
        public const float NeedleRayDistance = 350f;
        public const float AirNeedleGap = 4.62f;
        public const float NeedleCatchDistance = 1f;
        public const float CatchDeceleration = 0.75f;
        public const float DistantImpactRange = 15f;
    }
}
