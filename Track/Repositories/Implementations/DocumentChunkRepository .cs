using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Track.Repositories.Implementations
{
    public class DocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly AppDbContext _context;

        public DocumentChunkRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId)
        {
            return await _context.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .ToListAsync();
        }
    }
}
