using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Track.Repositories.Implementations
{
    public class QueryLogRepository : IQueryLogRepository
    {
        private readonly AppDbContext _context;

        public QueryLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(QueryLog log)
        {
            _context.QueryLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
