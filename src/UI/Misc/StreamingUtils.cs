using eft_dma_radar.Common.Misc;
using System;
using System.Threading.Tasks;

namespace eft_dma_radar.UI.Misc
{
    /// <summary>
    /// Static utility methods for streaming status checking
    /// </summary>
    public static class StreamingUtils
    {
        /// <summary>
        /// Check if a streamer is currently live based on their platform and username.
        /// Always returns false as external streaming service checks have been removed.
        /// </summary>
        public static Task<bool> IsLive(StreamingPlatform platform, string username)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Get a formatted streaming URL for the given platform and username
        /// </summary>
        public static string GetStreamingURL(StreamingPlatform platform, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return string.Empty;

            if (username.StartsWith("http"))
                return username;

            switch (platform)
            {
                case StreamingPlatform.Twitch:
                    return $"https://twitch.tv/{username}";

                case StreamingPlatform.YouTube:
                    return $"https://youtube.com/@{username}/live";

                default:
                    return string.Empty;
            }
        }
    }
}
