using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetAllWithItemsAsync();
    }
}