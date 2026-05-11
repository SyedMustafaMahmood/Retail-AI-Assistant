using Microsoft.AspNetCore.Mvc;
using Track.Services;
using Track.DTO;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _service;

        public TicketController(TicketService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] TicketRequest request)
        {
            var ticket = await _service.CreateTicketAsync(request);
            return Ok(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _service.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [HttpPost("{id}/summarize")]
        public async Task<IActionResult> Summarize(int id)
        {
            var result = await _service.SummarizeByIdAsync(id);
            if (result == null)
                return NotFound($"Ticket with ID {id} not found.");

            return Ok(result);
        }
    }
}