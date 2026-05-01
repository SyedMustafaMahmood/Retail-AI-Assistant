using Track.AI;
using Track.Data;
using Track.Models;

namespace Track.Services
{
    // Services/TicketService.cs
    public class TicketService
    {
        private readonly IAIClient _aiClient;
        private readonly AppDbContext _db;

        public TicketService(IAIClient aiClient, AppDbContext db)
        {
            _aiClient = aiClient;
            _db = db;
        }

        public async Task<string> SummarizeAsync(string ticket)
        {
            try
            {
                var prompt = $"Summarize this support ticket:\n{ticket}";

                var result = await _aiClient.GetCompletionAsync(prompt);

                _db.RequestLogs.Add(new RequestLog
                {
                    InputText = ticket,
                    OutputText = result,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = true
                });

                await _db.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _db.RequestLogs.Add(new RequestLog
                {
                    InputText = ticket,
                    OutputText = ex.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = false
                });

                await _db.SaveChangesAsync();

                throw;
            }
        }
    }
}
