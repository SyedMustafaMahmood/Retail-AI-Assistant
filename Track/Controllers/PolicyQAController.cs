using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Track.DTO;
using Track.Services;

[ApiController]

[Route("api/policy")]
//[AllowAnonymous]
public class PolicyQAController : ControllerBase
{
    private readonly IPolicyQAService _service;

    public PolicyQAController(IPolicyQAService service)
    {
        _service = service;
    }

    [Authorize(Roles = "SupportAgent")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please provide a valid document.");

        await _service.UploadDocumentAsync(file);
        return Ok(new { message = "Document uploaded successfully." });
    }

    [Authorize] // Any authenticated user (Customer or Agent)
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query cannot be empty.");

        var result = await _service.AskAsync(request.Query);
        return Ok(new { answer = result });
    }
}