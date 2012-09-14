using System;

namespace PPF.Models.ViewModels
{
    public class SeasonViewModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfWeeks { get; set; }
        public bool IsCurrent { get; set; }
        public LeaderboardViewModel Leaderboard { get; set; }
        public LeaderboardViewModel WeeklyLeaderboard { get; set; }
        public int SeasonPointTotal { get; set; }

    }
}