
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineChatBack.Models;
using OnlineChatBack.Repositories;
using Swashbuckle.AspNetCore.Filters;
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
                    policy.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader();
                });
            });


            builder.Services.AddControllers();

            builder.Services.AddDbContext<UserDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            builder.Services.AddSingleton<ChatRoomRepository>();

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

                options.SaveToken = true;
                options.Events = new JwtBearerEvents();
                options.Events.OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("token"))
                    {
                        context.Token = context.Request.Cookies["token"];
                    }

                    return Task.CompletedTask;
                };
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

            app.Run();
        }
    }
}
