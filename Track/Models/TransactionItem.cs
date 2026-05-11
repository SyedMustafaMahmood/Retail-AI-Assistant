namespace Track.Models
{
    public class TransactionItem
    {
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public string ProductName { get; set; }
    }
}
