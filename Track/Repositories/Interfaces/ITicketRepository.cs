using Track.Models;

namespace Track.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket);

        Task<List<Ticket>> GetAllAsync();

        Task<Ticket?> GetByIdAsync(int id);

        Task DeleteAsync(Ticket ticket);

        Task<List<Ticket>> GetByStatusAsync(string status);

        Task<List<Ticket>> GetByCustomerAsync(string customerName);

        Task SaveChangesAsync();
    }
}