using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IEmbeddingRepository
    {
        Task<List<EmbeddingMetadata>> GetAllAsync();
    }
}