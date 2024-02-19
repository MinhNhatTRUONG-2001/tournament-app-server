namespace tournament_app_server.Models
{
    public class Tournament
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTimeOffset? start_date { get; set; }
        public DateTimeOffset? end_date { get; set; }
        public string[]? places { get; set; }
        public int user_id { get; set; }
    }
}
