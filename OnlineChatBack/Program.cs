
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineChatBack.Hubs;
using OnlineChatBack.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace OnlineChatBack
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // cors
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://192.168.0.104:3000", "http://192.168.0.103")
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader();
                });
            });


            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatString = "yyyy.MM.dd HH:mm";
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddSignalR();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Cookie,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            // JWT
            var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenOptions:Key").Value!);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetSection("TokenOptions:Issuer").Value,
                    ValidAudience = builder.Configuration.GetSection("TokenOptions:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var claimsPrincipal = context.Principal;
                        var username = claimsPrincipal?.FindFirstValue(ClaimTypes.Name);

                        if(username == null)
                        {
                            context.Fail("Bad request");
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

                        var isUserInDb = await dbContext.Users.AnyAsync(user => user.Username == username);

                        if(!isUserInDb)
                        {
                            context.Fail("Unauthorized");
                        }

                    },

                    OnMessageReceived = context =>
                    {
                        if(context.Request.Cookies.ContainsKey("jwtToken"))
                        {
                            context.Token = context.Request.Cookies["jwtToken"];
                        }

                        return Task.CompletedTask;
                    }
                };

                options.SaveToken = true;

            }).AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            var app = builder.Build();
            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatRoomHub>("/chat-room-hub");

            app.Run();
        }
    }
}
