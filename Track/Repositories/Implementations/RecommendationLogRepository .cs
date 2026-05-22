using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class RecommendationLogRepository : IRecommendationLogRepository
    {
        private readonly AppDbContext _db;

        public RecommendationLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task AddRangeAsync(List<RecommendationLog> logs)
        {
            _db.RecommendationLogs.AddRange(logs);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}