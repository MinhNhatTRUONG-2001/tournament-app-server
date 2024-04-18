namespace tournament_app_server.Models
{
    public class MatchRr : Match
    {
        public short leg_number { get; set; }
        public decimal? team_1_score { get; set; }
        public decimal? team_2_score { get; set; }
        public decimal[]? team_1_subscores { get; set; }
        public decimal[]? team_2_subscores { get; set; }
        public decimal[]? team_1_other_criteria_values { get; set; }
        public decimal[]? team_2_other_criteria_values { get; set; }
    }
}
