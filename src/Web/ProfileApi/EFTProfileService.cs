using eft_dma_radar.Common.DMA;
using eft_dma_radar.Common.Misc;
using eft_dma_radar.UI.Misc;
using System.Net.Http;

namespace eft_dma_radar.Tarkov.API
{
    public static class EFTProfileService
    {
        #region Fields / Constructor
        private static readonly HttpClient _client;
        private static readonly Lock _syncRoot = new();
        private static readonly ConcurrentDictionary<string, ProfileData> _profiles = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _tdevNotFound = new(StringComparer.OrdinalIgnoreCase);

        private static CancellationTokenSource _cts = new();

        /// <summary>
        /// Persistent Cache Access.
        /// </summary>
        private static ProfileApiCache Cache => Program.Config.Cache.ProfileAPI;

        static EFTProfileService()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            new Thread(Worker)
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            }.Start();
            MemDMA.GameStarted += MemDMA_GameStarted;
            MemDMA.GameStopped += MemDMA_GameStopped;
        }

        private static void MemDMA_GameStopped(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = new();
            }
        }

        private static void MemDMA_GameStarted(object sender, EventArgs e)
        {
            uint pid = Memory.Process.PID;
            if (Cache.PID != pid)
            {
                Cache.PID = pid;
                Cache.Profiles.Clear();
            }
        }

        #endregion

        #region Public API
        /// <summary>
        /// Profile data returned by the Tarkov API.
        /// </summary>
        public static IReadOnlyDictionary<string, ProfileData> Profiles => _profiles;
        /// <summary>
        /// Attempt to register a Profile for lookup.
        /// </summary>
        /// <param name="accountId">Profile's Account ID.</param>
        public static void RegisterProfile(string accountId) => _profiles.TryAdd(accountId, null);

        #endregion

        #region Internal API
        private static async void Worker()
        {
            while (true)
            {
                if (MemDMABase.WaitForProcess())
                {
                    try
                    {
                        CancellationToken ct;
                        lock (_syncRoot)
                        {
                            ct = _cts.Token;
                        }
                        var profiles = _profiles
                            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Value is null)
                            .Select(x => x.Key);
                        if (profiles.Any())
                        {
                            foreach (var accountId in profiles)
                            {
                                ct.ThrowIfCancellationRequested();
                                await GetProfileAsync(accountId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        XMLogging.WriteLine($"[EFTProfileService] ERROR: {ex}");
                    }
                    finally { await Task.Delay(250); } // Rate-Limit
                }
            }
        }

        /// <summary>
        /// Get profile data for a particular Account ID.
        /// NOT thread safe. Always await this method and only run from one thread.
        /// </summary>
        /// <param name="accountId">Account ID of profile to lookup.</param>
        /// <returns></returns>
        private static async Task GetProfileAsync(string accountId)
        {
            if (Cache.Profiles.TryGetValue(accountId, out var cachedProfile))
            {
                _profiles[accountId] = cachedProfile;
                return;
            }

            try
            {
                ProfileData profile = null;

                profile = await LookupFromTarkovDevAsync(accountId);

                if (profile != null || _tdevNotFound.Contains(accountId))
                {
                    Cache.Profiles[accountId] = profile;
                }

                _profiles[accountId] = profile;
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
            }
        }

        /// <summary>
        /// Perform a BEST-EFFORT profile lookup via Tarkov.dev
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static async Task<ProfileData> LookupFromTarkovDevAsync(string accountId)
        {
            const string baseUrl = "https://players.tarkov.dev/profile/"; // [profileid].json
            try
            {
                if (_tdevNotFound.Contains(accountId))
                {
                    return null;
                }
                string url = baseUrl + accountId + ".json";
                using var response = await _client.GetAsync(url);
                if (response.StatusCode is HttpStatusCode.NotFound)
                {
                    XMLogging.WriteLine($"[EFTProfileService] Profile '{accountId}' not found by Tarkov.Dev.");
                    _tdevNotFound.Add(accountId);
                    return null;
                }
                if (response.StatusCode is HttpStatusCode.TooManyRequests) // Force Rate-Limit
                {
                    XMLogging.WriteLine("[EFTProfileService] Rate-Limited by Tarkov.Dev - Pausing for 1 minute.");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    return null;
                }
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ProfileData>(stream) ??
                    throw new ArgumentNullException("result");
                XMLogging.WriteLine($"[EFTProfileService] Got Profile '{accountId}' via Tarkov.Dev!");
                return result;
            }
            catch (Exception ex)
            {
                XMLogging.WriteLine($"[EFTProfileService] Unhandled ERROR looking up profile '{accountId}' via Tarkov.Dev: {ex}");
                return null;
            }
        }

        #region Profile Response JSON Structure

        public sealed class ProfileData
        {
            [JsonPropertyName("info")]
            public ProfileInfo Info { get; set; }

            [JsonPropertyName("pmcStats")]
            public StatsContainer PmcStats { get; set; }

            [JsonPropertyName("scavStats")]
            public StatsContainer ScavStats { get; set; }

            [JsonPropertyName("achievements")]
            public Dictionary<string, long> Achievements { get; set; }

            [JsonPropertyName("updated")]
            public long Updated { get; set; }
        }

        public sealed class ProfileInfo
        {
            [JsonPropertyName("nickname")]
            public string Nickname { get; set; }

            [JsonPropertyName("experience")]
            public int Experience { get; set; }

            [JsonPropertyName("memberCategory")]
            public int MemberCategory { get; set; }

            [JsonPropertyName("prestigeLevel")]
            public int Prestige { get; set; }

            [JsonPropertyName("registrationDate")]
            public int RegistrationDate { get; set; }

        }

        public sealed class StatsContainer
        {
            [JsonPropertyName("eft")]
            public CountersContainer Counters { get; set; }
        }

        public sealed class CountersContainer
        {
            [JsonPropertyName("totalInGameTime")]
            public int TotalInGameTime { get; set; }

            [JsonPropertyName("overAllCounters")]
            public OverallCounters OverallCounters { get; set; }
        }

        public sealed class OverallCounters
        {
            [JsonPropertyName("Items")]
            public List<OverAllCountersItem> Items { get; set; }
        }

        public sealed class OverAllCountersItem
        {
            [JsonPropertyName("Key")]
            public List<string> Key { get; set; } = new();

            [JsonPropertyName("Value")]
            public int Value { get; set; }
        }
        #endregion

        #endregion
    }
}