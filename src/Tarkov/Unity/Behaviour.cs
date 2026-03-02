
namespace eft_dma_radar.Common.Unity.LowLevel
{
    public readonly struct Behaviour
    {
        public static implicit operator ulong(Behaviour x) => x.Base;
        private readonly ulong Base;
    }
}