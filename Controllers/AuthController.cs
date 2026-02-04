using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using noteapp.Data;
using noteapp.DTOs;
using noteapp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace noteapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ✅ POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User Registered Successfully",
                user = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            });
        }

        // ✅ POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Invalid Email");

            bool validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!validPassword)
                return BadRequest("Invalid Password");

            string token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login Successful",
                token = token,
                user = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            });
        }

        // 🔥 JWT Token Generator
        private string GenerateJwtToken(User user)
        {
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    var expireMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES");

    if (string.IsNullOrEmpty(jwtKey))
        throw new Exception("JWT_KEY missing in .env file");

    if (string.IsNullOrEmpty(jwtIssuer))
        throw new Exception("JWT_ISSUER missing in .env file");

    if (string.IsNullOrEmpty(jwtAudience))
        throw new Exception("JWT_AUDIENCE missing in .env file");

    if (string.IsNullOrEmpty(expireMinutes))
        expireMinutes = "60"; // default fallback

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(Convert.ToDouble(expireMinutes)),
        signingCredentials: creds
    );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
