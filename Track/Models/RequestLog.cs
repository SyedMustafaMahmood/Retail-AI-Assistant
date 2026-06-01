namespace Track.Models
{
    public class RequestLog
    {
        public int Id { get; set; }
        public string InputText { get; set; }
        public string OutputText { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}
