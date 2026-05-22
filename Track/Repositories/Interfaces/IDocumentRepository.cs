using Track.Models;
namespace Track.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document);
        Task<Document?> GetLatestAsync();
    }
}
