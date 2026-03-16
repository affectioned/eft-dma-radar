using eft_dma_radar.Common.DMA;
using eft_dma_radar.Common.DMA.Features;
using eft_dma_radar.Common.Misc;

namespace eft_dma_radar.Tarkov.Features
{
    /// <summary>
    /// Feature Manager Thread.
    /// </summary>
    internal static class FeatureManager
    {
        internal static void ModuleInit()
        {
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
            XMLogging.WriteLine("Features Thread Starting...");

            while (true)
            {
                try
                {
                    // Wait for process to be up (blocks until GameStarted)
                    if (!MemDMABase.WaitForProcess())
                    {
                        Thread.Sleep(250);
                        continue;
                    }
                    bool ready = Memory.Ready;
                    bool inRaid = Memory.InRaid;
                    bool hasLocal = Memory.LocalPlayer is not null;
                    bool handsValid = hasLocal &&
                                      Memory.LocalPlayer.Firearm.HandsController.Item1.IsValidVirtualAddress();

                    if (!ready || !inRaid || !hasLocal || !handsValid)
                    {
                        // Gate silently, no spam logging
                        Thread.Sleep(250);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    XMLogging.WriteLine($"[Features Thread] CRITICAL ERROR: {ex}");
                }
                finally
                {
                    // Small back-off before restarting the outer loop
                    Thread.Sleep(1000);
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
