namespace tournament_app_server.Models
{
    public class Tournament
    {
        public long id { get; set; }
        public string name { get; set; }
        public DateTimeOffset? start_date { get; set; }
        public DateTimeOffset? end_date { get; set; }
        public string[]? places { get; set; }
        public long user_id { get; set; }
        public string? description { get; set; }
        public bool is_private { get; set; }
    }
}
