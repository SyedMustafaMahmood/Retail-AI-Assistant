using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Repositories.Implementations
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _db;

        public TicketRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Ticket ticket)
        {
            await _db.Tickets.AddAsync(ticket);
        }

        public async Task<List<Ticket>> GetAllAsync()
        {
            return await _db.Tickets
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _db.Tickets.FindAsync(id);
        }

        public async Task DeleteAsync(Ticket ticket)
        {
            _db.Tickets.Remove(ticket);
            await Task.CompletedTask;
        }

        public async Task<List<Ticket>> GetByStatusAsync(string status)
        {
            return await _db.Tickets
                .Where(t => t.Status.ToLower() == status.ToLower())
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetByCustomerAsync(string customerName)
        {
            return await _db.Tickets
                .Where(t => t.CustomerName == customerName)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}