namespace Track.AI
{
    public interface IEmbeddingClient
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }
}
