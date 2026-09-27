using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SilkSoarDash
{
    public static class SilkLog
    {
        private const string Prefix = "SSD";

        private static readonly Dictionary<Type, ManualLogSource> Sources = new Dictionary<Type, ManualLogSource>();

        public static ManualLogSource For<T>()
        {
            return For(typeof(T));
        }

        public static ManualLogSource For(Type type)
        {
            if (Sources.TryGetValue(type, out var source))
            {
                return source;
            }

            source = Logger.CreateLogSource(Prefix + "." + type.Name);
            Sources.Add(type, source);

            return source;
        }
    }
}
