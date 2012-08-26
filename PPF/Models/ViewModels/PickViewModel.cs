using System.Collections.Generic;

namespace PPF.Models.ViewModels
{
    public class PickViewModel
    {
        public int Id { get; set; }
        public GameViewModel Game { get; set; }
        public int PointTotal { get; set; }
        public string UserId { get; set; }
        public TeamViewModel Team { get; set; }
        public bool IsStandard { get; set; }
        public bool IsWinner { get; set; }
        public List<int> PossiblePoints { get; set; }
    }
}