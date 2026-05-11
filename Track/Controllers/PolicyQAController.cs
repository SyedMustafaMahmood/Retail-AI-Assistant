using Microsoft.AspNetCore.Mvc;
using Track.DTO;
using Track.Services;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/policy")]
    public class PolicyQAController : ControllerBase
    {
        private readonly IPolicyQAService _service;

        public PolicyQAController(IPolicyQAService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            await _service.UploadDocumentAsync(file);
            return Ok("Document uploaded successfully.");
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            var result = await _service.AskAsync(request.Query);
            return Ok(result);
        }
    }
}