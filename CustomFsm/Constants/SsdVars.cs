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
        public const string WallHitLeft = "WallHitLeft";
        public const string WallHitRight = "WallHitRight";
        public const string DidAddUsingSilk = "DidAddUsingSilk";
        
        public const int DefaultThrowDistance = 9;
        public const float DefaultJumpSpeed = 33f;
        public const float DefaultChargeTime = 0.8f;
        public const float ThrowWaitTimeOffScreen = 0.5f;
        public const float NeedleThrowTimeout = 0.8f;
        public const float ThreadLoopDelay = 0.1f;
        public const float ThreadRayDistance = 10f;
        public const int TerrainLayer = 8;
        public const float DefaultCancelableTime = 0.2f;
        public const float StoppedSpeed = 0.1f;

        public const float NeedleStartHeight = 0.85f;
        public const int ShortThrowThreshold = 12;
        public const float NeedleThrowSpeed = 150f;
        public const float RetractNeedleSpeed = 150f;
        public const float CancelDeceleration = 0.98f;
        public const float NeedleDamagerRange = 30f;
        public const float NeedleReturnedDistance = -0.1f;
        public const float NeedleRayDistance = 350f;
        public const float DistantImpactRange = 15f;
        
        public const int SilkCost = 1;
    }
}
