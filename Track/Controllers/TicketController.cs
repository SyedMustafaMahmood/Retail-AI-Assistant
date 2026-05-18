using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Track.DTO;
using Track.Services;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/tickets")]

    [Authorize]
    //[AllowAnonymous ]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _service;

        public TicketController(TicketService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] TicketRequest request)
        {
            var ticket = await _service.CreateTicketAsync(request);
            return Ok(ticket);
        }
        [Authorize(Roles = "SupportAgent,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _service.GetAllTicketsAsync();
            return Ok(tickets);
        }
        [Authorize(Roles = "SupportAgent")]
        [HttpPost("{id}/summarize")]
        public async Task<IActionResult> Summarize(int id)
        {
            var result = await _service.SummarizeByIdAsync(id);
            if (result == null)
                return NotFound($"Ticket with ID {id} not found.");

            return Ok(result);
        }
       // [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var user = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var ticket = await _service.GetTicketByIdAsync(id, user!, role!);

            if (ticket == null)
                return Forbid(); // or NotFound depending on design

            return Ok(ticket);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var result = await _service.DeleteTicketAsync(id);

            if (!result)
                return NotFound($"Ticket with ID {id} not found.");

            return Ok("Ticket deleted successfully");
        }


        [Authorize(Roles = "SupportAgent,Admin")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var tickets = await _service.GetTicketsByStatusAsync(status);
            return Ok(tickets);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            var username = User.Identity?.Name;

            var tickets = await _service.GetMyTicketsAsync(username!);

            return Ok(tickets);
        }
    }
}