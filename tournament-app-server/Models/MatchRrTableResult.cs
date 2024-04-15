namespace tournament_app_server.Models
{
    public class MatchRrTableResult
    {
        public string name {  get; set; }
        public decimal points { get; set; }
        public decimal difference { get; set; }
        public decimal accumulated_score { get; set; }
        public decimal[] other_criteria_values { get; set; }
    }
}
