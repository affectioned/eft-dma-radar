#nullable enable

namespace eft_dma_radar.Tarkov.EFTPlayer.Plugins
{
    public sealed class PlayerProfile
    {
        private readonly ObservedPlayer _player;
        public PlayerProfile(ObservedPlayer player)
        {
            _player = player;
        }

        public string? Nickname => null;
        public int Prestige => -1;
        public float? Overall_KD => null;
        public int? RaidCount => null;
        public float? SurvivedRate => null;
        public int? RunThroughCount => null;
        public int? ScavSessions => null;
        public Dictionary<string, long> Achievements => new Dictionary<string, long>();
        public int AchievementCount => 0;
        public string? Updated => null;
        public int? Hours => null;
        public int? Level => null;
        public Enums.EMemberCategory? MemberCategory => null;
        public string Acct => "--";
    }
}
