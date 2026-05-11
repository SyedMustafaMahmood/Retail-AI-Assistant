namespace Track.Models
{
    public class QueryLog
    {
        public Guid Id { get; set; }
        public string Query { get; set; }
        public string Response { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
