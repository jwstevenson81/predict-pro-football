using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPF.Models.ViewModels
{
    public class CurrentSeasonJsonModel
    {
        public int SeasonYear { get; set; }
        public int Id { get; set; }
        public string Season
        {
            get
            {
                if (SeasonYear > 0)
                    return string.Concat(SeasonYear.ToString(), " Season");
                else
                    return "No Season Selected";
            }
        }
    }
}