namespace SilkSoarDash.CustomFsm
{
    public static class SsdHeroState
    {
        public static bool Dashing { get; set; }
        public static bool OnWall { get; set; }
        public static bool ExitedDashing { get; set; }
        public static bool CrossingRoom { get; set; }
        public static bool CancelQueued { get; set; }
        public static float ExitHeight { get; set; }
        public static int SilkCost { get; set; }
    }
}