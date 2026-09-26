namespace SilkSoarDash.CustomFsm
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
        
        public const int DefaultThrowDistance = 9;
        public const float DefaultJumpSpeed = 33f;
        
        public const float NeedleStartHeight = 0.85f;
        public const int ShortThrowThreshold = 12;
    }
}
