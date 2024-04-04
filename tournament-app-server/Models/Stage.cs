namespace tournament_app_server.Models
{
    public class Stage
    {
        public long id { get; set; }
        public string name { get; set; }
        public short format_id { get; set; }
        public DateTimeOffset? start_date { get; set; }
        public DateTimeOffset? end_date { get; set; }
        public string[]? places { get; set; }
        public long tournament_id { get; set; }
        public short number_of_teams_per_group { get; set; }
        public short number_of_groups {  get; set; }
        public short stage_order { get; set; }
        public bool include_third_place_match { get; set; }
        public short[] number_of_legs_per_round { get; set; }
        public short[] best_of_per_round { get; set; }
        public short? third_place_match_number_of_legs { get; set; }
        public short? third_place_match_best_of { get; set; }
        public string? description { get; set; }
    }
}
