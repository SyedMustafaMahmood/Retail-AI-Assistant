using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IDocumentChunkRepository
    {
        Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId);
    }
}
