using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPF.Models.ViewModels
{
    public class SuperbowlPlayoffPickerViewModel
    {
        public List<SuperbowlPlayoffPickViewModel> Picks { get; set; }
        public List<TeamViewModel> Teams { get; set; }
    }
}