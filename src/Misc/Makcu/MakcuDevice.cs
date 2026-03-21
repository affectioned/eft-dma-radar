using eft_dma_radar.Common.Misc;
using System.Runtime.InteropServices;
using static eft_dma_radar.Misc.Makcu.MakcuNative;

namespace eft_dma_radar.Misc.Makcu
{
    /// <summary>
    /// Managed wrapper around the makcu-cpp native device handle.
    /// Dispose to release the native resource.
    /// </summary>
    public sealed class MakcuDevice : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;

        public bool IsConnected => !_disposed && _handle != IntPtr.Zero && makcu_is_connected(_handle);

        public MakcuDevice()
        {
            _handle = makcu_device_create();
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("makcu_device_create() returned null.");
        }

        /// <summary>
        /// Connect to the MAKCU device on the given COM port (e.g. "COM3").
        /// </summary>
        public void Connect(string port)
        {
            Check(makcu_connect(_handle, port));
            XMLogging.WriteLine($"[Makcu] Connected on {port}");
        }

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        public void Disconnect()
        {
            Check(makcu_disconnect(_handle));
            XMLogging.WriteLine("[Makcu] Disconnected");
        }

        /// <summary>
        /// Move the mouse by (x, y) pixels instantly.
        /// </summary>
        public void MouseMove(int x, int y) =>
            Check(makcu_mouse_move(_handle, x, y));

        /// <summary>
        /// Move the mouse smoothly over <paramref name="steps"/> increments with <paramref name="delayMs"/> ms between each.
        /// </summary>
        public void MouseMoveSmooth(int x, int y, int steps = 20, int delayMs = 1) =>
            Check(makcu_mouse_move_smooth(_handle, x, y, steps, delayMs));

        /// <summary>
        /// Move along a Bézier curve defined by two control points.
        /// </summary>
        public void MouseMoveBezier(int x, int y, int ctrl1X, int ctrl1Y, int ctrl2X, int ctrl2Y, int steps = 30, int delayMs = 1) =>
            Check(makcu_mouse_move_bezier(_handle, x, y, ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, steps, delayMs));

        /// <summary>Press a mouse button down.</summary>
        public void MouseDown(MakcuMouseButton button = MakcuMouseButton.Left) =>
            Check(makcu_mouse_down(_handle, button));

        /// <summary>Release a mouse button.</summary>
        public void MouseUp(MakcuMouseButton button = MakcuMouseButton.Left) =>
            Check(makcu_mouse_up(_handle, button));

        /// <summary>Click a mouse button (down + up).</summary>
        public void Click(MakcuMouseButton button = MakcuMouseButton.Left) =>
            Check(makcu_mouse_click(_handle, button));

        /// <summary>Scroll the mouse wheel. Positive = up, negative = down.</summary>
        public void Scroll(int amount) =>
            Check(makcu_mouse_wheel(_handle, amount));

        // ── Private helpers ──────────────────────────────────────────────────

        private static void Check(MakcuError err)
        {
            if (err == MakcuError.Success)
                return;

            var msgPtr = makcu_error_string(err);
            var msg = msgPtr != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(msgPtr) ?? err.ToString()
                : err.ToString();

            throw new MakcuException(err, msg);
        }

        // ── IDisposable ──────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_handle != IntPtr.Zero)
            {
                if (makcu_is_connected(_handle))
                    makcu_disconnect(_handle);

                makcu_device_destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }

    public sealed class MakcuException : Exception
    {
        public MakcuError ErrorCode { get; }

        public MakcuException(MakcuError code, string message)
            : base($"Makcu error [{code}]: {message}")
        {
            ErrorCode = code;
        }
    }
}
