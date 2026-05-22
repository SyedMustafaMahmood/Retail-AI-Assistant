using Microsoft.EntityFrameworkCore;
using System.Text;
using Track.AI;
using Track.DTO;
using Track.Helpers;
using Track.Models;
using Track.Repositories.Implementations;
using Track.Repositories.Interfaces;

namespace Track.Services
{
    public class TicketService
    {
        private readonly IAIClient _aiClient;
        private readonly ITicketRepository _ticketRepo;
        private readonly IRequestLogRepository _logRepo;

        public TicketService(
            IAIClient aiClient,
            ITicketRepository ticketRepo,
            IRequestLogRepository logRepo)
        {
            _aiClient = aiClient;
            _ticketRepo = ticketRepo;
            _logRepo = logRepo;
        }

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

            await _ticketRepo.AddAsync(ticket);
            await _ticketRepo.SaveChangesAsync();

            return ticket;
        }

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _ticketRepo.GetAllAsync();
        }

        public async Task<TicketSummaryResponse?> SummarizeByIdAsync(int id)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return null;

            try
            {
                var template = MarkdownLoader.Load("TicketSummarization.md");

                var prompt = MarkdownLoader.Replace(template,
                    new Dictionary<string, string>
                    {
                        { "customer", ticket.CustomerName },
                        { "subject", ticket.Subject },
                        { "description", ticket.Description }
                    });

                var summary = await _aiClient.GetCompletionAsync(prompt);

                await _logRepo.AddAsync(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = summary,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = true
                });

                ticket.Status = "Reviewed";

                await _ticketRepo.SaveChangesAsync();

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
                await _logRepo.AddAsync(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = ex.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = false
                });

                await _ticketRepo.SaveChangesAsync();

                throw;
            }
        }

        public async Task StreamSummaryByIdAsync(
    int id,
    HttpResponse response)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
            {
                response.StatusCode = 404;
                await response.WriteAsync("Ticket not found");
                return;
            }

            try
            {
                var template = MarkdownLoader.Load("TicketSummarization.md");

                var prompt = MarkdownLoader.Replace(template,
                    new Dictionary<string, string>
                    {
                { "customer", ticket.CustomerName },
                { "subject", ticket.Subject },
                { "description", ticket.Description }
                    });

                var finalSummary = new StringBuilder();

                await foreach (var chunk in _aiClient.GetCompletionStreamAsync(prompt))
                {
                    finalSummary.Append(chunk);

                    await response.WriteAsync(chunk);

                    await response.Body.FlushAsync();
                }

                await _logRepo.AddAsync(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = finalSummary.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = true
                });

                ticket.Status = "Reviewed";

                await _ticketRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logRepo.AddAsync(new RequestLog
                {
                    InputText = ticket.Description,
                    OutputText = ex.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = false
                });

                await _ticketRepo.SaveChangesAsync();

                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(
            int id,
            string username,
            string role)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return null;

            if (role == "Admin" || role == "SupportAgent")
                return ticket;

            if (role == "Customer" &&
                ticket.CustomerName == username)
                return ticket;

            return null;
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return false;

            await _ticketRepo.DeleteAsync(ticket);

            await _ticketRepo.SaveChangesAsync();

            return true;
        }

        public async Task<List<Ticket>> GetTicketsByStatusAsync(string status)
        {
            return await _ticketRepo.GetByStatusAsync(status);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return false;

            ticket.Status = status;

            await _ticketRepo.SaveChangesAsync();

            return true;
        }

        public async Task<List<Ticket>> GetMyTicketsAsync(string customerName)
        {
            return await _ticketRepo.GetByCustomerAsync(customerName);
        }

        // Update ticket status
        public async Task<bool> UpdateTicketStatusAsync(int id, string status)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return false;

            ticket.Status = status;
            await _db.SaveChangesAsync();

            return true;
        }
    }
}