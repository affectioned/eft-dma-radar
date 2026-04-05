#nullable enable
using eft_dma_radar.Common.Misc.Data;
using HandyControl.Tools.Extension;
using System.Threading;

namespace eft_dma_radar.Tarkov.EFTPlayer.Plugins
{
    public sealed class PlayerProfile
    {
        private readonly ObservedPlayer _player;
        public PlayerProfile(ObservedPlayer player)
        {
            _player = player;
        }

        private float? _overallKD;
        /// <summary>
        /// Player's Overall KD (only human players).
        /// </summary>
        public float? Overall_KD => null;

        public int? _raidCount;
        /// <summary>
        /// Player's Overall Raid Count (only human players).
        /// </summary>
        public int? RaidCount => null;

        private float? _survivedRate;
        /// <summary>
        /// Player's Overall Survival Percentage (only human players).
        /// </summary>
        public float? SurvivedRate => null;

        private int? _runThroughCount;
        /// Player's PMC Run Through Count (only human players).
        public int? RunThroughCount => null;

        private int? _scavSessions;
        /// Player's Scav Session Count (only human players).
        public int? ScavSessions => null;

        /// Player's Achievements Dictionary (only human players).
        public Dictionary<string, long> Achievements => new Dictionary<string, long>();

        /// Player's Total Achievement Count (only human players).
        public int AchievementCount => 0;

        private string? _updated;
        public string? Updated => null;

        private int? _hours;
        /// <summary>
        /// Player's Total Hours Played (only human players).
        /// </summary>
        public int? Hours => null;

        private int? _level;
        /// <summary>
        /// Player's In-Game Level (only human players).
        /// </summary>
        public int? Level => null;

        private Enums.EMemberCategory? _memberCategory;
        /// <summary>
        /// Player's Member Category (Standard/EOD/Dev/Sherpa, etc. -- only human players).
        /// </summary>
        public Enums.EMemberCategory? MemberCategory => null;

        /// <summary>
        /// True if this player is on an EOD Edition Account.
        /// </summary>
        private bool IsEOD => false;

        /// <summary>
        /// True if this player is on an Unheard Edition Account.
        /// </summary>
        private bool IsUnheard => false;

        /// <summary>
        /// Account type (eod, uh, etc.)
        /// </summary>
        public string Acct => "--";

        /// <summary>
        /// Player's Nickname (via Profile Data).
        /// </summary>
        public string? Nickname => null;

        public int Prestige => -1;

        /// <summary> Is the player flagged as streamer. </summary>
        public bool IsStreamer => false;

        /// <summary> Human-readable last updated. </summary>
        public string LastUpdatedReadable => "N/A";
    }
}
