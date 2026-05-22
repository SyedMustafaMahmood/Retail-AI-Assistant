using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class EmbeddingRepository : IEmbeddingRepository
    {
        private readonly AppDbContext _db;

        public EmbeddingRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<EmbeddingMetadata>> GetAllAsync()
        {
            return _db.Embeddings.ToListAsync();
        }
    }
}