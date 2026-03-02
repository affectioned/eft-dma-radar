using eft_dma_radar.Common.Unity;
using eft_dma_radar.Tarkov.EFTPlayer.Plugins;

namespace eft_dma_radar.Common.Maps
{
    /// <summary>
    /// Defines an entity that can be drawn on the 2D Radar Map.
    /// </summary>
    public interface IMapEntity : IWorldEntity
    {
        /// <summary>
        /// Draw this Entity on the Radar Map.
        /// </summary>
        /// <param name="canvas">SKCanvas instance to draw on.</param>
        void Draw(SKCanvas canvas, XMMapParams mapParams, ILocalPlayer localPlayer);
    }
}
