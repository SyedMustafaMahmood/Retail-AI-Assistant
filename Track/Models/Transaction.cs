namespace Track.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public List<TransactionItem> Items { get; set; } = new();
    }
}
