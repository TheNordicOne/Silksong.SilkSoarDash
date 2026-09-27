namespace SilkSoarDash.CustomFsm
{
    // The flash starts in one action and is cancelled in another, and its handle fits in no FSM variable.
    public static class SsdFlash
    {
        private static SpriteFlash.FlashHandle _handle;

        public static void StartSuperDash()
        {
            var flash = HeroController.instance.GetComponent<SpriteFlash>();
            if (flash == null)
            {
                return;
            }

            _handle = flash.FlashingSuperDashHandled();
        }

        public static void Cancel()
        {
            if (_handle.Equals(default))
            {
                return;
            }

            var flash = HeroController.instance.GetComponent<SpriteFlash>();
            if (flash != null)
            {
                flash.CancelRepeatingFlash(_handle);
            }

            _handle = default;
        }
    }
}
