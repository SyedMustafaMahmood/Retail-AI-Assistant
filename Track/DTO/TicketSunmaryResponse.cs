namespace Track.DTO
{
    public class TicketSummaryResponse
    {
        public int TicketId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}