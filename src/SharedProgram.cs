using eft_dma_radar.Common.Misc.Config;
using System.IO;
using System.Net.Http;

namespace eft_dma_radar
{
    /// <summary>
    /// Encapsulates a Shared State between this satelite module and the main application.
    /// </summary>
    public static class SharedProgram
    {
        private const string _mutexID = "0f908ff7-e614-6a93-60a3-cee36c9cea91";
#pragma warning disable IDE0052 // Remove unread private members
        private static Mutex _mutex;
#pragma warning restore IDE0052 // Remove unread private members

        internal static DirectoryInfo ConfigPath { get; private set; }
        internal static IConfig Config { get; private set; }
        /// <summary>
        /// Singleton HTTP Client for this application.
        /// </summary>
        public static HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Initialize the Shared State between this module and the main application.
        /// </summary>
        /// <param name="configPath">Config path directory.</param>
        /// <param name="config">Config file instance.</param>
        /// <exception cref="ApplicationException"></exception>
        public static void Initialize(DirectoryInfo configPath, IConfig config)
        {
            ArgumentNullException.ThrowIfNull(configPath, nameof(configPath));
            ArgumentNullException.ThrowIfNull(config, nameof(config));
            ConfigPath = configPath;
            Config = config;
            _mutex = new Mutex(true, _mutexID, out bool singleton);
            if (!singleton)
                throw new ApplicationException("The Application Is Already Running!");
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            SetupHttpClient();
#if !DEBUG
            VerifyDependencies();
#endif
        }

        /// <summary>
        /// Setup the HttpClient for this App Domain.
        /// </summary>
        private static void SetupHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            SharedProgram.HttpClient = client;
        }

        /// <summary>
        /// Validates that all startup dependencies are present.
        /// </summary>
        private static void VerifyDependencies()
        {
            // MemProcFS / LeechCore
            CheckDep("vmm.dll");
            CheckDep("leechcore.dll");
            CheckDep("leechcore_driver.dll");
            CheckDep("FTD3XX.dll");
            CheckDep("tinylz4.dll");
            CheckDep("dbghelp.dll");
            // VC++ runtime (portable — not required if installed system-wide)
            CheckDep("vcruntime140.dll");
            // SkiaSharp
            CheckDep("libSkiaSharp.dll");
            // Makcu
            CheckDep("makcu-cpp.dll");
        }

        private static void CheckDep(string fileName)
        {
            var exeDir = AppContext.BaseDirectory;
            var fullPath = Path.Combine(exeDir, fileName);
            if (!File.Exists(fullPath) && !File.Exists(fileName))
                throw new FileNotFoundException($"Missing Dependency '{fileName}'\n\n" +
                                                $"==Troubleshooting==\n" +
                                                $"1. Make sure that you unzipped the Client Files, and that all files are present in the same folder as the Radar Client (EXE).\n" +
                                                $"2. Expected location: {fullPath}");
        }

        /// <summary>
        /// Called when AppDomain is shutting down.
        /// </summary>
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e) =>
            Config.Save();

        public static void UpdateConfig(IConfig newConfig)
        {
            ArgumentNullException.ThrowIfNull(newConfig, nameof(newConfig));
            Config = newConfig;
        }
    }
}
