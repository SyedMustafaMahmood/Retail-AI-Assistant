using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IQueryLogRepository
    {
        Task AddAsync(QueryLog log);
    }
}
