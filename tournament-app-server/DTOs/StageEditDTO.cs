namespace tournament_app_server.DTOs
{
    public class StageEditDTO
    {
        public string name { get; set; }
        public long tournament_id { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string[]? places { get; set; }
        public string? description { get; set; }
    }
}
