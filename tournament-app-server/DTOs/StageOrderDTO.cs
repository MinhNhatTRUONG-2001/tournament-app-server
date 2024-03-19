namespace tournament_app_server.DTOs
{
    public class StageOrderDTO
    {
        public long id { get; set; }
        public string name { get; set; }
        public long tournament_id { get; set; }
        public short stage_order { get; set; }
    }
}
