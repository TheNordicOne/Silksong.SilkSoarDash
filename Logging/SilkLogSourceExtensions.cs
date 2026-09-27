using BepInEx.Logging;

namespace SilkSoarDash.Logging
{
    public static class SilkLogSourceExtensions
    {
        public static void Debug(this ManualLogSource source, string template, params object[] args)
        {
            source.LogDebug(new SilkLogEvent(template, args));
        }

        public static void Info(this ManualLogSource source, string template, params object[] args)
        {
            source.LogInfo(new SilkLogEvent(template, args));
        }

        public static void Warning(this ManualLogSource source, string template, params object[] args)
        {
            source.LogWarning(new SilkLogEvent(template, args));
        }

        public static void Error(this ManualLogSource source, string template, params object[] args)
        {
            source.LogError(new SilkLogEvent(template, args));
        }
    }
}
