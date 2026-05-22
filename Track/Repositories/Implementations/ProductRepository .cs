using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<Product>> GetAllAsync()
        {
            return _db.Products.ToListAsync();
        }

        public Task<List<Product>> GetByNamesAsync(List<string> names)
        {
            var lower = names.Select(x => x.ToLower()).ToList();

            return _db.Products
                .Where(p => lower.Contains(p.Name.ToLower()))
                .ToListAsync();
        }

        public Task<Product?> GetByNameAsync(string name)
        {
            return _db.Products
                .FirstOrDefaultAsync(p => p.Name == name);
        }
    }
}