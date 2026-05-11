namespace Track.Models
{
    public class RecommendationLog
    {
        public int Id { get; set; }

        public string RequestedProduct { get; set; }

        public string RecommendedProduct { get; set; }

        public double Confidence { get; set; }

        public double Similarity { get; set; }

        public double FinalScore { get; set; }

        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}