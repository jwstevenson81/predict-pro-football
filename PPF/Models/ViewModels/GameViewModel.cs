using System;

namespace PPF.Models.ViewModels
{
    public class GameViewModel
    {
        public TeamViewModel HomeTeam { get; set; }
        public TeamViewModel AwayTeam { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public int Id { get; set; }
        public bool IsPlayoff { get; set; }
        public bool IsSuperbowl { get; set; }
        public DateTime GameDateTime { get; set; }
    }
}