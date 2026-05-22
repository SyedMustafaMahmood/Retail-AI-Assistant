using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByNamesAsync(List<string> names);
        Task<Product?> GetByNameAsync(string name);
    }
}