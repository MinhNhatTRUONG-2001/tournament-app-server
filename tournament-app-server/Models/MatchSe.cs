namespace tournament_app_server.Models
{
    public class MatchSe : Match
    {
        public short round_number { get; set; }
        public decimal[]? team_1_scores { get; set; }
        public decimal[]? team_2_scores { get; set; }
        public decimal[]? team_1_subscores { get; set; }
        public decimal[]? team_2_subscores { get; set; }
        public short number_of_legs { get; set; }
        public short best_of { get; set; }
    }
}
