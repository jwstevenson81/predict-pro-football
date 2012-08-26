using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPF.Models.ViewModels
{
    public class LeaderViewModel
    {
        public string UserId { get; set; }
        public int Points { get; set; }
    }

    public class LeaderboardViewModel
    {
        public List<LeaderViewModel> Leaders { get; set; }

        public LeaderboardViewModel()
        {
            Leaders = new List<LeaderViewModel>();
        }
    }
}