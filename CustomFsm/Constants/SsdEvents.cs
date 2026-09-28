namespace SilkSoarDash.CustomFsm.Constants
{
    public static class SsdEvents
    {
        public const string Start = "START";
        public const string Finished = "FINISHED";
        public const string Cancelled = "CANCELLED";
        public const string ThrowNeedleStart = "THROW_NEEDLE_START";
        public const string GetDistance = "GET_DISTANCE";
        public const string ThrowNeedle = "THROW_NEEDLE";
        public const string DamagerHitSpikes = "DAMAGER HIT SPIKES";
        public const string TransitionGate = "TransitionGate";
        public const string HitWall = "HIT_WALL";
        public const string HeroDamaged = "HERO DAMAGED";
        public const string FsmCancel = "FSM CANCEL";
        public const string LeavingScene = "LEAVING SCENE";
        public const string EnterSprinting = "ENTER SPRINTING";
        public const string EnterDashing = "SSD ENTER DASHING";
    }
}