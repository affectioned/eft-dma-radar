using eft_dma_radar.Common.Misc;

namespace eft_dma_radar.Misc.Makcu
{
    /// <summary>
    /// Singleton manager for the Makcu device connection.
    /// </summary>
    public static class MakcuManager
    {
        private static MakcuDevice _device;
        private static readonly Lock _sync = new();

        /// <summary>
        /// True if the Makcu device is currently connected.
        /// </summary>
        public static bool IsConnected => _device?.IsConnected ?? false;

        /// <summary>
        /// The active Makcu device, or null if not connected.
        /// </summary>
        public static MakcuDevice Device => _device;

        /// <summary>
        /// Connect to the Makcu device on the given COM port (e.g. "COM3").
        /// Disposes any existing connection first.
        /// </summary>
        public static void Connect(string port)
        {
            lock (_sync)
            {
                _device?.Dispose();
                _device = null;

                var dev = new MakcuDevice();
                dev.Connect(port);
                _device = dev;

                Log.WriteLine($"[MakcuManager] Connected on {port}");
            }
        }

        /// <summary>
        /// Disconnect and dispose the current device.
        /// </summary>
        public static void Disconnect()
        {
            lock (_sync)
            {
                _device?.Dispose();
                _device = null;
                Log.WriteLine("[MakcuManager] Disconnected");
            }
        }
    }
}
