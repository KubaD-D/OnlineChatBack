using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineChatBack.Dtos;
using OnlineChatBack.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnlineChatBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _userDbContext;

        public UserController(IConfiguration configuration, UserDbContext userDbContext)
        {
            _configuration = configuration;
            _userDbContext = userDbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            if(await _userDbContext.Users.AnyAsync(user => user.Username == register.Username))
            {
                return Conflict("User with this username already exists");
            }

            if(await _userDbContext.Users.AnyAsync(user => user.Email == register.Email))
            {
                return Conflict("An account with this email address already exists");
            }

            var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(register.Password);

            var newUser = new User
            {
                Username = register.Username,
                Email = register.Email,
                IsEmailConfirmed = false,
                PasswordHash = passwordHash
            };

            await _userDbContext.Users.AddAsync(newUser);
            await _userDbContext.SaveChangesAsync();

            return Ok();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userDbContext.Users.FirstOrDefaultAsync(user => user.Username == login.Username);

            if(user == null)
            {
                return Unauthorized();
            }

            if(!BCrypt.Net.BCrypt.EnhancedVerify(login.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            await _userDbContext.SaveChangesAsync();

            var jwtToken = GenerateJwtToken(user.Username);

            return Ok(new { jwtToken, refreshToken });
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateJwtToken(string username)
        {
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("TokenOptions:Key").Value!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                   {
                        new Claim(ClaimTypes.Name, username),
                    }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = _configuration.GetSection("TokenOptions:Issuer").Value,
                Audience = _configuration.GetSection("TokenOptions:Audience").Value
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
