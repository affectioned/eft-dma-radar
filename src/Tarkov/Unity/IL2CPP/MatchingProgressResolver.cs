using System;
using System.Diagnostics;
using System.Threading;
using eft_dma_radar.Common.DMA;
using eft_dma_radar.Common.Misc;
using eft_dma_radar.Common.Misc.Data;
using eft_dma_radar.Common.Unity;
using SDK;

namespace eft_dma_radar.Tarkov.Unity.IL2CPP
{
    /// <summary>
    /// Resolves the MatchingProgressView instance via the IL2CPP TypeInfoTable
    /// and exposes the current EMatchingStage for the pre-raid waiting screen.
    /// </summary>
    internal static class MatchingProgressResolver
    {
        // Cached pointer to the MatchingProgressView MonoBehaviour instance.
        private static ulong _cachedMatchingProgress;
        private static readonly object _lock = new();

        // Cached matching stage, updated by TryUpdateStage()
        private static volatile int _cachedStage = (int)Enums.EMatchingStage.None;

        // Flag to avoid spawning multiple concurrent resolve tasks
        private static volatile bool _resolvingAsync;

        // Set to true once the raid starts; suppresses further stage polling
        private static volatile bool _raidStarted;

        /// <summary>
        /// Resets all cached state. Call at game stop and raid start/stop.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _cachedMatchingProgress = 0;
            }
            _cachedStage = (int)Enums.EMatchingStage.None;
            _resolvingAsync = false;
            _raidStarted = false;
        }

        /// <summary>
        /// Returns the last cached MatchingProgressView pointer.
        /// </summary>
        public static bool TryGetCached(out ulong matchingProgress)
        {
            lock (_lock)
            {
                matchingProgress = _cachedMatchingProgress;
                return matchingProgress.IsValidVirtualAddress();
            }
        }

        /// <summary>
        /// Returns the last cached EMatchingStage.
        /// </summary>
        public static Enums.EMatchingStage GetCachedStage()
        {
            return (Enums.EMatchingStage)_cachedStage;
        }

        /// <summary>
        /// Call once the raid has started to stop stage polling.
        /// </summary>
        public static void NotifyRaidStarted()
        {
            _raidStarted = true;
        }

        /// <summary>
        /// Fire-and-forget background resolve of the MatchingProgressView pointer.
        /// Safe to call from any thread.
        /// </summary>
        public static void ResolveAsync()
        {
            if (_resolvingAsync)
                return;

            _resolvingAsync = true;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = ResolveMatchingProgress();
                    if (result.IsValidVirtualAddress())
                    {
                        lock (_lock)
                        {
                            _cachedMatchingProgress = result;
                        }
                        Debug.WriteLine($"[MatchingProgressResolver] Resolved @ 0x{result:X}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MatchingProgressResolver] ResolveAsync error: {ex.Message}");
                }
                finally
                {
                    _resolvingAsync = false;
                }
            });
        }

        /// <summary>
        /// Reads the current EMatchingStage from the cached MatchingProgressView and
        /// updates the cached stage value. No-op if not yet resolved or raid started.
        /// </summary>
        public static void TryUpdateStage()
        {
            if (_raidStarted)
                return;

            if (!TryGetCached(out var mpView) || !mpView.IsValidVirtualAddress())
                return;

            try
            {
                // MatchingProgressView._matchingProgress is a reference to the MatchingProgress object
                var mpPtr = Memory.ReadPtr(mpView + SDK.Offsets.MatchingProgressView._matchingProgress, false);
                if (!mpPtr.IsValidVirtualAddress())
                    return;

                // MatchingProgress.CurrentStage is an int (enum) at offset 0x20
                var stage = Memory.ReadValue<int>(mpPtr + SDK.Offsets.MatchingProgress.CurrentStage);
                _cachedStage = stage;
            }
            catch
            {
                // Swallow — transient read failures are expected during loading
            }
        }

        // ── Internal resolution ──────────────────────────────────────────────────

        private static ulong ResolveMatchingProgress()
        {
            try
            {
                var gaBase = Memory.GameAssemblyBase;
                if (gaBase == 0)
                    return 0;

                // Read TypeInfoTable pointer
                var typeInfoTable = Memory.ReadPtr(gaBase + Offsets.Special.TypeInfoTableRva, false);
                if (!typeInfoTable.IsValidVirtualAddress())
                    return 0;

                // Use MatchingProgressView_TypeIndex to get the klass pointer
                ulong slot = typeInfoTable +
                    (ulong)Offsets.Special.MatchingProgressView_TypeIndex * (ulong)IntPtr.Size;

                var klassPtr = Memory.ReadPtr(slot, false);
                if (!klassPtr.IsValidVirtualAddress())
                    return 0;

                // Read static fields block from the klass
                var staticFields = Memory.ReadPtr(klassPtr + Offsets.Il2CppClass.StaticFields, false);
                if (!staticFields.IsValidVirtualAddress())
                    return 0;

                // The static field at offset 0x0 typically holds the singleton instance
                var instance = Memory.ReadPtr(staticFields, false);
                if (!instance.IsValidVirtualAddress())
                    return 0;

                return instance;
            }
            catch
            {
                return 0;
            }
        }
    }
}
