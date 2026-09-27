namespace SilkSoarDash.CustomFsm.Constants
{
    public static class SsdStates
    {
        public const string Inactive = "Inactive";
        public const string RelinquishControl = "RelinquishControl";
        public const string Charge = "SsdCharge";
        public const string Charged = "Charged";
        public const string Cancelled = "Cancelled";
        public const string GetDistance = "GetDistance";
        public const string ResetEffects = "ResetEffects";
        public const string ThrowNeedle = "ThrowNeedle";
        public const string HitSpikes = "HitSpikes";
        public const string PositionStickNeedlePre = "PositionStickNeedlePre";
        public const string HitTransitionGate = "HitTransitionGate";
        public const string PositionStickNeedle = "PositionStickNeedle";
        public const string ThrowWait = "ThrowWait";
        public const string DashAntic = "DashAntic";
        public const string DashStart = "DashStart";
        public const string Dashing = "Dashing";
        public const string Cancelable = "Cancelable";
        public const string HitWallHard = "HitWallHard";
        public const string HitWall = "HitWall";
        public const string RetractNeedleCancel = "RetractNeedleCancel";
        public const string RegainControlToIdle = "RegainControlToIdle";
    }
}
