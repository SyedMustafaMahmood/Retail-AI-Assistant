using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;

        public TransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<Transaction>> GetAllWithItemsAsync()
        {
            return _db.Transactions
                .Include(t => t.Items)
                .ToListAsync();
        }
    }
}