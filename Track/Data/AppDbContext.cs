using Microsoft.EntityFrameworkCore;
using Track.Models;
namespace Track.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<RequestLog> RequestLogs { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        public DbSet<QueryLog> QueryLogs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }
        public DbSet<EmbeddingMetadata> Embeddings { get; set; }
        public DbSet<RecommendationLog> RecommendationLogs { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

    }
}
