using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Track.Data;
using Track.Models;
using Track.DTO;
using Track.Services;

namespace Track.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var exists = await _db.Users.AnyAsync(x => x.Email == request.Email);

            if (exists)
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Customer"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registered Successfully (V2)"
            });
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid Email");

            bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!valid)
                return Unauthorized("Invalid Password");

            // Access token (JWT)
            var accessToken = _jwt.GenerateToken(user);

            // Refresh token
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var existingToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.Token);

            if (existingToken == null)
                return Unauthorized("Invalid refresh token");

            if (existingToken.IsRevoked)
                return Unauthorized("Refresh token already revoked");

            if (existingToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            // 🚀 Revoke old refresh token
            existingToken.IsRevoked = true;

            var user = await _db.Users
                .FindAsync(existingToken.UserId);

            if (user == null)
                return Unauthorized("User not found");

            // 🚀 Generate NEW access token
            var newAccessToken = _jwt.GenerateToken(user);

            // 🚀 Generate NEW refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _db.RefreshTokens.Add(newRefreshToken);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken.Token
            });
        }

        // ---------------- CREATE AGENT ----------------
        [Authorize(Roles = "Admin")]
        [HttpPost("create-agent")]
        public async Task<IActionResult> CreateAgent(RegisterRequest request)
        {
            var exists = await _db.Users.AnyAsync(x => x.Email == request.Email);

            if (exists)
                return BadRequest("Email already exists");

            var agent = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "SupportAgent"
            };

            _db.Users.Add(agent);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Support Agent Created"
            });
        }
    }
}