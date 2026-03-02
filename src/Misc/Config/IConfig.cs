using eft_dma_radar.Common.Unity.LowLevel;

namespace eft_dma_radar.Common.Misc.Config
{
    public interface IConfig
    {
        int MonitorWidth { get; }
        int MonitorHeight { get; }

        void Save();
        Task SaveAsync();
    }
}
