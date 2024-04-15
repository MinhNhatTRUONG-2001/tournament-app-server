namespace tournament_app_server.DTOs
{
    public class MatchSeEditMatchScoreDTO
    {
        public string? winner { get; set; }
        public decimal[]? team_1_scores { get; set; }
        public decimal[]? team_2_scores { get; set; }
        public decimal[]? team_1_subscores { get; set; }
        public decimal[]? team_2_subscores { get; set; }
    }
}
