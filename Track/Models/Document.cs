namespace Track.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<DocumentChunk> Chunks { get; set; }
    }
}
