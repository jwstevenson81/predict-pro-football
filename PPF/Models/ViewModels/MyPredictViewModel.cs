using System;
using System.Collections.Generic;
using System.Linq;

namespace PPF.Models.ViewModels
{
    public class MyPredictViewModel
    {
        public SeasonViewModel TheSeason { get; set; }
        public int CurrentWeek { get; set; }
        public List<PickViewModel> MyPicks { get; set; }
        public List<SuperbowlPlayoffPickViewModel> MyPlayoffPicks { get; set; }
        public List<SuperbowlPlayoffPickViewModel> MySuperbowlPicks { get; set; }
        public List<PickViewModel> StandardWeeklyPicks { get; set; }
        public List<SuperbowlPlayoffPickViewModel> StandardPlayoffPicks { get; set; }
        public List<SuperbowlPlayoffPickViewModel> StandardSuperbowlPicks { get; set; }

        public List<TeamViewModel> AfcTeams { get; set; }
        public List<TeamViewModel> NfcTeams { get; set; }
        public bool Disabled { get; set; }
        public bool AreStandardPicks { get; set; }
        public int WeekPointTotal { get; set; }
        public int PlayoffSuperbowlPointTotal { get; set; }
        public int PossiblePlayoffPointTotal { get; set; }
        public int PossibleSuperbowlPointTotal { get; set; }
        public bool ShouldHavePlayoffSuperbowlPicks { get; set; }
        public bool ShouldHaveSuperbowlPicksOnly { get; set; }

        public List<int> PointList { get; set; }

        public MyPredictViewModel()
        {
            MyPicks = new List<PickViewModel>();
            MyPlayoffPicks = new List<SuperbowlPlayoffPickViewModel>();
            MySuperbowlPicks = new List<SuperbowlPlayoffPickViewModel>();
            StandardWeeklyPicks = new List<PickViewModel>();
            StandardPlayoffPicks = new List<SuperbowlPlayoffPickViewModel>();
            StandardSuperbowlPicks = new List<SuperbowlPlayoffPickViewModel>();
        }
    }
}