using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IRequestLogRepository
    {
        Task AddAsync(RequestLog log);
    }
}