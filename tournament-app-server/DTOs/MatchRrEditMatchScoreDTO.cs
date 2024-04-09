namespace tournament_app_server.DTOs
{
    public class MatchRrEditMatchScoreDTO
    {
        public string? winner { get; set; }
        public long? team_1_score { get; set; }
        public long? team_2_score { get; set; }
        public long[]? team_1_subscores { get; set; }
        public long[]? team_2_subscores { get; set; }
    }
}
