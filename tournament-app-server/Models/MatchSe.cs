using System.Text.Json.Serialization;
using System.Text.Json;

namespace tournament_app_server.Models
{
    public class MatchSe
    {
        public long id {  get; set; }
        public long stage_id { get; set; }
        public short round_number { get; set; }
        public short match_number { get; set; }
        public string? team_1 { get; set; }
        public string? team_2 { get; set; }
        public string? winner { get; set; }
        public long[]? team_1_scores { get; set; }
        public long[]? team_2_scores { get; set; }
        public long[]? team_1_subscores { get; set; }
        public long[]? team_2_subscores { get; set; }
        public short best_of { get; set; }
        public short group_number { get; set; }
        public DateTimeOffset? start_datetime { get; set; }
        public string? place {  get; set; }
        public string? note { get; set; }
    }
}
