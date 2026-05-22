using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;

namespace Track.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/v2/products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _db.Products.ToListAsync();
            return Ok(products);
        }
    }
}