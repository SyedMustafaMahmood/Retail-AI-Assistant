using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Track.Data;
using Track.Models;
using Track.Services;
using Track.DTO;
using Asp.Versioning;

namespace Track.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(
            AppDbContext db,
            JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            var exists = await _db.Users
                .AnyAsync(x =>
                    x.Email == request.Email);

            if (exists)
                return BadRequest(
                    "Email already exists");

            var user = new User
            {
                Name = request.Name,

                Email = request.Email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                Role = "Customer"
            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync();

            return Ok("Registered Successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email);

            if (user == null)
                return Unauthorized(
                    "Invalid Email");

            bool valid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!valid)
                return Unauthorized(
                    "Invalid Password");

            var token =
                _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("create-agent")]
        public async Task<IActionResult> CreateAgent(RegisterRequest request)
        {
            var exists = await _db.Users
                .AnyAsync(x => x.Email == request.Email);

            if (exists)
                return BadRequest("Email already exists");

            var agent = new User
            {
                Name = request.Name,

                Email = request.Email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                Role = "SupportAgent"
            };

            _db.Users.Add(agent);

            await _db.SaveChangesAsync();

            return Ok("Support Agent Created");
        }
    }
}