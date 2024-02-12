using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineChatBack.Dtos;
using OnlineChatBack.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            if(login.Username == "admin" && login.Password == "password" || login.Username == "string" && login.Password == "string")
            {
                var token = GenerateJwtToken(login.Username);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.Now.AddDays(1),
                };

                Response.Cookies.Append("token", token, cookieOptions);

                return Ok();
            }

            return Unauthorized();
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
                Expires = DateTime.Now.AddDays(1),
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
