namespace Track.Models
{
    public class RecommendationResult
    {
        public string Product { get; set; }

        public double Confidence { get; set; }

        public double Similarity { get; set; }

        public double FinalScore { get; set; }

        public string Reason { get; set; }
    }
}