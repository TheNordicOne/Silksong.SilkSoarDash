using System.Collections.Generic;
using HarmonyLib;

namespace SilkSoarDash.CustomFsm
{
    // the spool only ever marks its top chunk as in use, however much is reserved, so the rest of the cost is marked here
    public static class SsdSilkReserve
    {
        private static readonly AccessTools.FieldRef<SilkSpool, List<SilkChunk>> SilkChunks = AccessTools.FieldRefAccess<SilkSpool, List<SilkChunk>>("silkChunks");
        private static readonly List<SilkChunk> MarkedChunks = new List<SilkChunk>();

        private static int _reserved;

        private static SilkSpool Spool => SilkSpool.Instance;

        public static bool Reserve(int cost)
        {
            _reserved = cost;
            return Spool.AddUsing(SilkSpool.SilkUsingFlags.Normal, cost);
        }

        public static void Release(int cost)
        {
            _reserved = 0;
            Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, cost);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SilkSpool), nameof(SilkSpool.RefreshSilk), typeof(SilkSpool.SilkAddSource), typeof(SilkSpool.SilkTakeSource))]
        private static void RefreshSilkPrefix()
        {
            foreach (var chunk in MarkedChunks)
            {
                if (chunk)
                {
                    chunk.PlayIdle();
                }
            }

            MarkedChunks.Clear();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SilkSpool), nameof(SilkSpool.RefreshSilk), typeof(SilkSpool.SilkAddSource), typeof(SilkSpool.SilkTakeSource))]
        // ReSharper disable once InconsistentNaming - Harmony Postfix Matching
        private static void RefreshSilkPostfix(SilkSpool __instance)
        {
            // the game itself marks the first one
            var remaining = _reserved - 1;
            var chunks = SilkChunks(__instance);
            var skippedGameChunk = false;

            for (var i = chunks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var chunk = chunks[i];
                if (chunk.IsRegen)
                {
                    continue;
                }

                if (!skippedGameChunk)
                {
                    skippedGameChunk = true;
                    continue;
                }

                chunk.SetUsing(SilkSpool.SilkUsingFlags.Normal);
                MarkedChunks.Add(chunk);
                remaining--;
            }
        }
    }
}
