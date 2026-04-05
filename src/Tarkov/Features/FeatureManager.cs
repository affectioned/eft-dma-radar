#pragma warning disable CS0162 // Unreachable code detected (HARD_DISABLE_ALL_MEMWRITES const)
using eft_dma_radar.Common.Misc;
using eft_dma_radar.Tarkov.GameWorld;

using eft_dma_radar.Common.DMA;
using eft_dma_radar.Common.DMA.ScatterAPI;
using eft_dma_radar.Common.DMA.Features;
using eft_dma_radar.UI.Misc;
using eft_dma_radar.Tarkov.EFTPlayer;
using eft_dma_radar.Misc.Makcu;

namespace eft_dma_radar.Tarkov.Features
{
    /// <summary>
    /// Feature Manager Thread.
    /// </summary>
    internal static class FeatureManager
    {
        /// <summary>
        /// HARD DISABLE - Set to true to completely disable ALL memory writes.
        /// This overrides all config settings. For development/safety.
        /// </summary>
        private const bool HARD_DISABLE_ALL_MEMWRITES = false;

        internal static void ModuleInit()
        {
            // Force eager initialization of all feature singletons so they are
            // registered in IFeature.AllFeatures before GameStarted fires.
            _ = Aimbot.Instance;

            // Auto-connect Makcu on startup if configured.
            var aimbotCfg = (SharedProgram.Config as Config)?.Aimbot;
            if (aimbotCfg?.AutoConnect == true && !string.IsNullOrWhiteSpace(aimbotCfg.MakcuPort))
            {
                try { MakcuManager.Connect(aimbotCfg.MakcuPort); }
                catch (Exception ex) { Log.WriteLine($"[Makcu] Auto-connect failed: {ex.Message}"); }
            }

            new Thread(Worker)
            {
                IsBackground = true,
                Name = "FeatureManager"
            }.Start();
        }

        static FeatureManager()
        {
            MemDMABase.GameStarted += Memory_GameStarted;
            MemDMABase.GameStopped += Memory_GameStopped;
            MemDMABase.RaidStarted += Memory_RaidStarted;
            MemDMABase.RaidStopped += Memory_RaidStopped;
        }

        private static void Worker()
        {
            Log.WriteLine("Features Thread Starting...");

            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"[Features Thread] CRITICAL ERROR: {ex}");
                }
            }
        }

        private static void Memory_GameStarted(object sender, EventArgs e)
        {
            foreach (var feature in IFeature.AllFeatures)
            {
                feature.OnGameStart();
            }
        }

        private static void Memory_GameStopped(object sender, EventArgs e)
        {
            foreach (var feature in IFeature.AllFeatures)
            {
                feature.OnGameStop();
            }
        }

        private static void Memory_RaidStarted(object sender, EventArgs e)
        {
            foreach (var feature in IFeature.AllFeatures)
            {
                feature.OnRaidStart();
            }
        }

        private static void Memory_RaidStopped(object sender, EventArgs e)
        {
            foreach (var feature in IFeature.AllFeatures)
            {
                feature.OnRaidEnd();
            }
        }
    }

}
