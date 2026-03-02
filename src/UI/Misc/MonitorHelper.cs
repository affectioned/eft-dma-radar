using eft_dma_radar.Common.Misc;
using System.Windows;

namespace eft_dma_radar.UI.Misc
{
    public class MonitorHelper
    {
        public class MonitorInfo
        {
            public Rect Bounds { get; set; }
            public bool IsPrimary { get; set; }
        }

        public static List<MonitorInfo> GetAllMonitors()
        {
            var result = new List<MonitorInfo>();
            try
            {
                foreach (var screen in Screen.AllScreens)
                {
                    result.Add(new MonitorInfo
                    {
                        Bounds = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height),
                        IsPrimary = screen.Primary
                    });

                    XMLogging.WriteLine($"[MonitorHelper] Monitor: {screen.Bounds.Width}x{screen.Bounds.Height}, Primary: {screen.Primary}");
                }
            }
            catch (Exception ex)
            {
                XMLogging.WriteLine($"[MonitorHelper] Failed to fetch monitors: {ex}");
            }

            return result;
        }
    }

}
