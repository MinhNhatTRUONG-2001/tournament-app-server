namespace tournament_app_server.DTOs
{
    public class TournamentDTO
    {
        public string name { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string[]? places { get; set; }
        public string? description { get; set; }
        public bool is_private { get; set; }
    }
}
