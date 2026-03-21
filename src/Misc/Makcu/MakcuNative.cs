using System.Runtime.InteropServices;

namespace eft_dma_radar.Misc.Makcu
{
    // ── Public enums (namespace-level so public classes can reference them) ──

    public enum MakcuError : int
    {
        Success = 0,
        InvalidHandle = 1,
        NotConnected = 2,
        AlreadyConnected = 3,
        ConnectionFailed = 4,
        Timeout = 5,
        InvalidParameter = 6,
        DeviceNotFound = 7,
        CommandFailed = 8,
        OutOfMemory = 9,
    }

    public enum MakcuMouseButton : int
    {
        Left = 0,
        Right = 1,
        Middle = 2,
        Side1 = 3,
        Side2 = 4,
    }

    /// <summary>
    /// Raw P/Invoke bindings for makcu-cpp.dll (C API).
    /// https://github.com/K4HVH/makcu-cpp
    /// </summary>
    internal static class MakcuNative
    {
        private const string DLL = "makcu-cpp.dll";

        // ── Device lifecycle ─────────────────────────────────────────────────
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr makcu_device_create();

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void makcu_device_destroy(IntPtr device);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern MakcuError makcu_connect(IntPtr device, string port);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_disconnect(IntPtr device);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool makcu_is_connected(IntPtr device);

        // ── Mouse movement ───────────────────────────────────────────────────
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_move(IntPtr device, int x, int y);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_move_smooth(IntPtr device, int x, int y, int steps, int delayMs);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_move_bezier(IntPtr device,
            int x, int y,
            int ctrl1X, int ctrl1Y,
            int ctrl2X, int ctrl2Y,
            int steps, int delayMs);

        // ── Mouse buttons ────────────────────────────────────────────────────
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_down(IntPtr device, MakcuMouseButton button);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_up(IntPtr device, MakcuMouseButton button);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_click(IntPtr device, MakcuMouseButton button);

        // ── Scroll wheel ─────────────────────────────────────────────────────
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MakcuError makcu_mouse_wheel(IntPtr device, int amount);

        // ── Utilities ────────────────────────────────────────────────────────
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr makcu_error_string(MakcuError error);
    }
}
