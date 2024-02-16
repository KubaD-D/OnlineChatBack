using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _applicationDbContext;

        public UserController(IConfiguration configuration, ApplicationDbContext applicationDbContext)
        {
            _configuration = configuration;
            _applicationDbContext = applicationDbContext;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto register)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            if(await _applicationDbContext.Users.AnyAsync(user => user.Username == register.Username))
            {
                return Conflict("User with this username already exists");
            }

            if(await _applicationDbContext.Users.AnyAsync(user => user.Email == register.Email))
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

            await _applicationDbContext.Users.AddAsync(newUser);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(newUser);

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user => user.Username == login.Username);

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
            await _applicationDbContext.SaveChangesAsync();

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshTokenExpiry
            };

            Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);

            var jwtToken = GenerateJwtToken(user.Username);

            var jwtCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            Response.Cookies.Append("jwtToken", jwtToken, jwtCookieOptions);

            return Ok(new { user.Username });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var expiredToken = HttpContext.Request.Cookies["jwtToken"];

            if(expiredToken == null)
            {
                return BadRequest();
            }

            var principal = GetPrincipalFromExpiredToken(expiredToken);

            if(principal?.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user =>  user.Username == principal.Identity.Name);

            var refreshToken = HttpContext.Request.Cookies["refreshToken"];

            if(user == null || refreshToken == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            var jwtToken = GenerateJwtToken(principal.Identity.Name);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            Response.Cookies.Append("jwtToken", jwtToken, cookieOptions);

            return Ok(new { Username = principal.Identity.Name });
        }

        [HttpDelete("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var username = HttpContext.User.Identity?.Name;

            if(username == null)
            {
                return Unauthorized();
            }

            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user => user.Username == username);

            if(user == null)
            {
                return Unauthorized();
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Delete("refreshToken", cookieOptions);
            Response.Cookies.Delete("jwtToken", cookieOptions);

            user.RefreshToken = null;
            await _applicationDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(PasswordDto request)
        {
            var username = HttpContext.User.Identity?.Name;

            if(username == null)
            {
                return BadRequest();
            }

            var user = _applicationDbContext.Users.FirstOrDefault(user => user.Username == username);

            if(user == null)
            {
                return NotFound();
            }

            if(!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            _applicationDbContext.Remove(user);
            await _applicationDbContext.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Delete("refreshToken", cookieOptions);
            Response.Cookies.Delete("jwtToken", cookieOptions);

            return Ok();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(PasswordChangeDto request)
        {
            var username = HttpContext.User?.Identity?.Name;

            if(username == null || request.OldPassword == request.NewPassword)
            {
                return BadRequest();
            }

            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user => user.Username == username);

            if(user == null)
            {
                return NotFound();
            }

            if(!BCrypt.Net.BCrypt.EnhancedVerify(request.OldPassword, user.PasswordHash))
            {
                return Unauthorized();
            }

            var newPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword);

            user.PasswordHash = newPasswordHash;

            await _applicationDbContext.SaveChangesAsync();

            return Ok();

        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("TokenOptions:Key").Value!);

            var validation = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration.GetSection("TokenOptions:Issuer").Value,
                ValidAudience = _configuration.GetSection("TokenOptions:Audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
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
