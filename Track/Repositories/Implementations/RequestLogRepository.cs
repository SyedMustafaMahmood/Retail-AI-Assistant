using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class RequestLogRepository : IRequestLogRepository
    {
        private readonly AppDbContext _db;

        public RequestLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(RequestLog log)
        {
            await _db.RequestLogs.AddAsync(log);
        }
    }
}