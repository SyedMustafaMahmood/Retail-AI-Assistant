using Microsoft.EntityFrameworkCore;
using Track.Helpers;
using Track.AI;
using Track.Data;
using Track.Models;

namespace Track.Services
{
    public class EmbeddingBuilderService
    {
        private readonly AppDbContext _db;
        private readonly IEmbeddingClient _embedding;

        public EmbeddingBuilderService(AppDbContext db, IEmbeddingClient embedding)
        {
            _db = db;
            _embedding = embedding;
        }

        public async Task BuildAsync()
        {
            if (await _db.Embeddings.AnyAsync())
                return;

            var products = await _db.Products.ToListAsync();

            foreach (var product in products)
            {
                var vector = await _embedding.GetEmbeddingAsync(product.Description);

                _db.Embeddings.Add(new EmbeddingMetadata
                {
                    ProductName = product.Name,
                    Content = product.Description,
                    Vector = VectorHelper.ToVectorString(vector)
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}