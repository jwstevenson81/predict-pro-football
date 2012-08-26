namespace PPF.Models.ViewModels
{
    public class SuperbowlPlayoffPickViewModel
    {
        public int Id { get; set; }
        public bool IsPlayoff { get; set; }
        public bool IsSuperbowl { get; set; }
        public bool IsStandard { get; set; }
        public bool IsWinner { get; set; }
        public int PointTotal { get; set; }
        public string UserId { get; set; }
        public TeamViewModel Team { get; set; }
    }
}