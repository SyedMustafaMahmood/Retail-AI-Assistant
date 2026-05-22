using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Track.DTO;
using Track.Services;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/tickets")]

    [Authorize]
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
        [Authorize(Roles = "SupportAgent,Admin")]
        [HttpPost("{id}/summarize")]
        public async Task<IActionResult> Summarize(int id)
        {
            var result = await _service.SummarizeByIdAsync(id);
            if (result == null)
                return NotFound($"Ticket with ID {id} not found.");

            return Ok(result);
        }
        [Authorize(Roles = "SupportAgent")]
        [HttpPost("{id}/summarize-stream")]
        public async Task SummarizeStream(int id)
        {
            HttpContext.Features
                .Get<IHttpResponseBodyFeature>()?
                .DisableBuffering();

            Response.Headers.Append("Content-Type", "text/plain");
            Response.Headers.Append("Cache-Control", "no-cache");

            await _service.StreamSummaryByIdAsync(id, Response);
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
        [Authorize(Roles ="SupportAgent")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusRequest request)
        {
            var result = await _service.UpdateStatusAsync(id, request.Status);

            if (!result)
                return NotFound($"Ticket {id} not found");

            return Ok("Status updated successfully");
        }


        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            var username = User.Identity?.Name;

            var tickets = await _service.GetMyTicketsAsync(username!);

            return Ok(tickets);
        }

        
        [Authorize(Roles = "SupportAgent,Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var result = await _service.UpdateTicketStatusAsync(id, request.Status);
            if (!result)
                return NotFound($"Ticket with ID {id} not found.");
            return Ok("Status updated successfully.");
        }
    }
}