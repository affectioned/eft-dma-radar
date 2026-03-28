using eft_dma_radar.Common.Unity.LowLevel;

namespace eft_dma_radar.Common.Misc.Config
{
    public interface IConfig
    {
        LowLevelCache LowLevelCache { get; }
        bool MemWritesEnabled { get; }
        int MonitorWidth { get; }
        int MonitorHeight { get; }

        void Save();
        Task SaveAsync();
    }
}
