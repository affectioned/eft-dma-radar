using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using eft_dma_radar.Common.DMA;
using eft_dma_radar.Common.Misc;
using SDK;

namespace eft_dma_radar.Tarkov.Unity.IL2CPP
{
    public static partial class Il2CppDumper
    {
        // ── TypeInfoTableRva resolution ──────────────────────────────────────────

        /// <summary>
        /// Attempts to resolve the TypeInfoTableRva by verifying the pointer at
        /// the current hardcoded offset is a valid virtual address.
        /// Updates <see cref="Offsets.Special.TypeInfoTableRva"/> if a better value
        /// is found.
        /// </summary>
        /// <param name="gaBase">GameAssembly.dll base address.</param>
        /// <param name="quiet">If true, suppresses failure log messages (for retry loops).</param>
        /// <returns>True if the TypeInfoTable pointer is valid; false otherwise.</returns>
        private static bool ResolveTypeInfoTableRva(ulong gaBase, bool quiet = false)
        {
            try
            {
                var tablePtr = Memory.ReadPtr(gaBase + Offsets.Special.TypeInfoTableRva, false);
                if (tablePtr.IsValidVirtualAddress())
                    return true;

                if (!quiet)
                    Log.WriteLine($"[Il2CppDumper] TypeInfoTableRva @ 0x{Offsets.Special.TypeInfoTableRva:X} resolved to invalid pointer 0x{tablePtr:X}.");

                return false;
            }
            catch (Exception ex)
            {
                if (!quiet)
                    Log.WriteLine($"[Il2CppDumper] ResolveTypeInfoTableRva failed: {ex.Message}");
                return false;
            }
        }

        // ── TypeIndex resolution ─────────────────────────────────────────────────

        /// <summary>
        /// Uses the name→index lookup built from the live type table to update
        /// the TypeIndex fields in <see cref="Offsets.Special"/>.
        /// This keeps the type-index values accurate when the game binary changes.
        /// </summary>
        /// <param name="nameToIndex">Dictionary mapping IL2CPP class names to their type table index.</param>
        private static void ResolveTypeIndices(Dictionary<string, int> nameToIndex)
        {
            const BindingFlags bf = BindingFlags.Public | BindingFlags.Static;
            var specialType = typeof(Offsets.Special);

            TryUpdateTypeIndex(specialType, bf, nameToIndex, "EFTHardSettings", "EFTHardSettings_TypeIndex");
            TryUpdateTypeIndex(specialType, bf, nameToIndex, "GPUInstancerManager", "GPUInstancerManager_TypeIndex");
            TryUpdateTypeIndex(specialType, bf, nameToIndex, "WeatherController", "WeatherController_TypeIndex");
            TryUpdateTypeIndex(specialType, bf, nameToIndex, "GlobalConfiguration", "GlobalConfiguration_TypeIndex");
            TryUpdateTypeIndex(specialType, bf, nameToIndex, "MatchingProgress", "MatchingProgress_TypeIndex");
            TryUpdateTypeIndex(specialType, bf, nameToIndex, "MatchingProgressView", "MatchingProgressView_TypeIndex");
        }

        private static void TryUpdateTypeIndex(
            Type specialType,
            BindingFlags bf,
            Dictionary<string, int> nameToIndex,
            string className,
            string fieldName)
        {
            if (!nameToIndex.TryGetValue(className, out int idx))
                return;

            var fi = specialType.GetField(fieldName, bf);
            if (fi is null || fi.IsLiteral)
                return;

            try
            {
                fi.SetValue(null, (uint)idx);
                Debug.WriteLine($"[Il2CppDumper] TypeIndex '{className}' → {idx}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Il2CppDumper] TryUpdateTypeIndex '{fieldName}': {ex.Message}");
            }
        }

        // ── Debug dump ───────────────────────────────────────────────────────────

        /// <summary>
        /// Logs a summary of the resolver run to the debug console.
        /// </summary>
        private static void DebugDumpResolverState(int classCount, int updated, int fallback, int skipped)
        {
            Debug.WriteLine(
                $"[Il2CppDumper] Resolver summary — classes: {classCount}, " +
                $"updated: {updated}, fallback: {fallback}, skipped: {skipped}");
        }
    }
}
