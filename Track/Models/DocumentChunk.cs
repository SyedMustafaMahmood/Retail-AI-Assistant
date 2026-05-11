namespace Track.Models
{
    public class DocumentChunk
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string Content { get; set; }
        public string EmbeddingJson { get; set; }
        public Document Document { get; set; } // ✅ add this

    }
}
