using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPF.Models.ViewModels
{
    public class SeasonSearchViewModel
    {
        // Existing repetitive properties
        public int Id { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime StartDate { get; set; }
        public string StartDateDisplay
        {
            get { return StartDate != DateTime.MinValue ? StartDate.ToShortDateString() : string.Empty; }
        }
        // display only properties
        public int Weeks { get; set; }
        public string Season { get; set; }
    }
}