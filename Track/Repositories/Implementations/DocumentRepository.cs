using Track.Data;
using Track.Repositories.Interfaces;
using Track.Models;
using Microsoft.EntityFrameworkCore;
namespace Track.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task<Document?> GetLatestAsync()
        {
            return await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync();
        }
    }
}
