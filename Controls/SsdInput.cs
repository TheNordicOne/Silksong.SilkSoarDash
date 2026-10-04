using SilkSoarDash.Config;

namespace SilkSoarDash.Controls
{
    public static class SsdInput
    {
        public static HeroActions Actions => GameManager.instance?.inputHandler?.inputActions;

        public static bool PressedSilkSoarDash()
        {
            return Actions != null && Actions.SuperDash.WasPressed && DashDirectionPressed();
        }

        public static bool PressedCancel()
        {
            return Actions != null && (Actions.Jump.WasPressed || Actions.Attack.WasPressed || Actions.SuperDash.WasPressed);
        }

        public static bool DashDirectionPressed()
        {
            if (Actions == null)
            {
                return false;
            }

            var direction = SsdConfig.SwapDirections ? Actions.Down : Actions.Up;
            return direction.IsPressed;
        }

        public static bool PressingOnlyUp()
        {
            return Actions.Up.IsPressed && !Actions.Left.IsPressed && !Actions.Right.IsPressed;
        }
    }
}
