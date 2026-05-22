using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IRecommendationLogRepository
    {
        Task AddRangeAsync(List<RecommendationLog> logs);
        Task SaveChangesAsync();
    }
}