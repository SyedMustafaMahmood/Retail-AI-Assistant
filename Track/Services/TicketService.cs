using Microsoft.EntityFrameworkCore;
using Track.AI;
using Track.Data;
using Track.DTO;
using Track.Helpers;
using Track.Models;

namespace Track.Services
{
    public class TicketService
    {
        private readonly IAIClient _aiClient;
        private readonly AppDbContext _db;

        public TicketService(IAIClient aiClient, AppDbContext db)
        {
            _aiClient = aiClient;
            _db = db;
        }

        //  Create ticket and save to DB
        public async Task<Ticket> CreateTicketAsync(TicketRequest request)
        {
            var ticket = new Ticket
            {
                CustomerName = request.CustomerName,
                Subject = request.Subject,
                Description = request.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return ticket;
        }

        //  Get all tickets from DB
        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _db.Tickets.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        //  Fetch ticket by ID → summarize via Gemini
        public async Task<TicketSummaryResponse?> SummarizeByIdAsync(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return null;

            try
            {
                var template = MarkdownLoader.Load("TicketSummarization.md");

                var prompt = MarkdownLoader.Replace(template, new Dictionary<string, string>
                {
                    { "customer", ticket.CustomerName },
                    { "subject", ticket.Subject },
                    { "description", ticket.Description }
                });

                var summary = await _aiClient.GetCompletionAsync(prompt);

                _db.RequestLogs.Add(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = summary,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = true
                });
                ticket.Status = "Reviewed";
                await _db.SaveChangesAsync();

                return new TicketSummaryResponse
                {
                    TicketId = ticket.Id,
                    CustomerName = ticket.CustomerName,
                    Subject = ticket.Subject,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                _db.RequestLogs.Add(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = ex.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = false
                });

                await _db.SaveChangesAsync();

                throw;
            }
        }
        //Get Ticket By ID
        public async Task<Ticket?> GetTicketByIdAsync(int id, string username, string role)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return null;

            // Admin & SupportAgent can see everything
            if (role == "Admin" || role == "SupportAgent")
                return ticket;

            // Customer can only see their own ticket
            if (role == "Customer" && ticket.CustomerName == username)
                return ticket;

            // Not allowed
            return null;
        }
        //delete ticket by id
        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return false;

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return true;
        }
        // get ticket by status 
        public async Task<List<Ticket>> GetTicketsByStatusAsync(string status)
        {
            return await _db.Tickets
                .Where(t => t.Status.ToLower() == status.ToLower())
                .ToListAsync();
        }

        //get my tickets
        public async Task<List<Ticket>> GetMyTicketsAsync(string customerName)
        {
            return await _db.Tickets
                .Where(t => t.CustomerName == customerName)
                .ToListAsync();
        }
    }
}