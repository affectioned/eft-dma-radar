using System.Net.Http.Json;

namespace eft_dma_radar.Common.Misc.Data.TarkovMarket
{
    internal static class TarkovDevCore
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<TarkovDevQuery> QueryTarkovDevAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(100));

            var results = await Task.WhenAll(
                QueryAsync("""{ maps { name nameId extracts { name faction position { x y z } } transits { description position { x y z } } } }""", cts.Token),
                QueryAsync("""{ items { id name shortName width height sellFor { vendor { name } priceRUB } basePrice avg24hPrice historicalPrices { price } categories { name } iconLink iconLinkFallback imageLink properties { ... on ItemPropertiesWeapon { caliber } } } }""", cts.Token),
                QueryAsync("""{ tasks { id name kappaRequired objectives { id type optional description maps { id name normalizedName } ... on TaskObjectiveItem { item { id name shortName } zones { id map { id normalizedName name } position { y x z } } requiredKeys { id name shortName } count foundInRaid } ... on TaskObjectiveMark { id description markerItem { id name shortName } maps { id normalizedName name } zones { id map { id normalizedName name } position { y x z } } requiredKeys { id name shortName } } ... on TaskObjectiveQuestItem { id description requiredKeys { id name shortName } maps { id normalizedName name } zones { id map { id normalizedName name } position { y x z } } questItem { id name shortName normalizedName description } count } ... on TaskObjectiveBasic { id description requiredKeys { id name shortName } maps { id normalizedName name } zones { id map { id normalizedName name } position { y x z } } } ... on TaskObjectiveShoot { maps { id normalizedName name } zones { id map { id normalizedName name } outline { x y z } position { y x z } } } } } }""", cts.Token),
                QueryAsync("""{ questItems { id shortName } lootContainers { id normalizedName name } }""", cts.Token)
            );

            return new TarkovDevQuery
            {
                Data = new TarkovDevQuery.TarkovDevData
                {
                    Maps = results[0].Data?.Maps,
                    Items = results[1].Data?.Items,
                    Tasks = results[2].Data?.Tasks,
                    QuestItems = results[3].Data?.QuestItems,
                    LootContainers = results[3].Data?.LootContainers,
                },
                Warnings = results.FirstOrDefault(r => r.Warnings != null)?.Warnings
            };
        }

        private static async Task<TarkovDevQuery> QueryAsync(string query, CancellationToken ct)
        {
            var payload = new Dictionary<string, string> { { "query", query } };
            using var response = await SharedProgram.HttpClient.PostAsJsonAsync(
                "https://api.tarkov.dev/graphql", payload, ct);
            response.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<TarkovDevQuery>(
                await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }
    }
}
