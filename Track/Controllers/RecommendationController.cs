using Microsoft.AspNetCore.Mvc;
using Track.DTO;
using Track.Services;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/recommend")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _service;

        public RecommendationController(RecommendationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Recommend([FromBody] RecommendationRequest request)
        {
            var result = await _service.RecommendAsync(request.Products);
            return Ok(result);
        }
    }
}