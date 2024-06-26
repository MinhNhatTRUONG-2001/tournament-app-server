﻿namespace tournament_app_server.DTOs
{
    public class MatchSeDTO
    {
        public long stage_id { get; set; }
        public short group_number { get; set; }
        public short round_number { get; set; }
        public short match_number { get; set; }
        public string? team_1 { get; set; }
        public string? team_2 { get; set; }
        public string? winner { get; set; }
        public decimal[]? team_1_scores { get; set; }
        public decimal[]? team_2_scores { get; set; }
        public decimal[]? team_1_subscores { get; set; }
        public decimal[]? team_2_subscores { get; set; }
        public short number_of_legs { get; set; }
        public short best_of { get; set; }
        public string? start_datetime { get; set; }
        public string? place { get; set; }
        public string? note { get; set; }
    }
}
