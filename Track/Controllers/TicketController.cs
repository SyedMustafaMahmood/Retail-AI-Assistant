using Microsoft.AspNetCore.Mvc;
using Track.Services;
using Track.DTO;

namespace Track.Controllers
{

    [ApiController]
    [Route("api/ticket")]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _service;

        public TicketController(TicketService service)
        {
            _service = service;
        }

        [HttpPost("summarize")]
        public async Task<IActionResult> Summarize([FromBody] TicketRequest request)
        {
            var result = await _service.SummarizeAsync(request.Ticket);
            return Ok(result);
        }
    }
}