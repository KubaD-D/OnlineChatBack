using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineChatBack.Dtos;
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

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto login)
        {
            if(login.Username == "admin" && login.Password == "password" || login.Username == "string" && login.Password == "string")
            {
                var key = Encoding.UTF8.GetBytes(_configuration.GetSection("TokenOptions:Key").Value!);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, login.Username),
                    }),
                    Expires = DateTime.Now.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                    Issuer = _configuration.GetSection("TokenOptions:Issuer").Value,
                    Audience = _configuration.GetSection("TokenOptions:Audience").Value
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { token = tokenHandler.WriteToken(token) });
            }

            return Unauthorized();
        }
    }
}
