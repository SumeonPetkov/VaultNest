
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NestVault.Api.Data;
using NestVault.Api.Interfaces;
using NestVault.Api.Services;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

namespace NestVault.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();



            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

            var myOptions = new MyRateLimitOptions();
            builder.Configuration.GetSection("RateLimitOptions").Bind(myOptions);

            builder.Services.AddRateLimiter(options =>
            {
                if (myOptions.EnableIpRateLimiting) {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 50,
                                Window = TimeSpan.FromMinutes(myOptions.TimeoutInMinutes)
                            }
                    ));
                }

                options.OnRejected = async (context, cancellationToken) =>
                {

                    // Custom rejection handling logic
                    context.HttpContext.Response.StatusCode = (int)Enum.GetValues(typeof(StatusCodes))
                                                                       .GetValue(myOptions.HttpStatusCode);
                    context.HttpContext.Response.Headers["Retry-After"] = (myOptions.TimeoutInMinutes * 60).ToString();

                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                };

            });


            var jwtKey = builder.Configuration["JwtOptions:Key"];
            var jwtIssuer = builder.Configuration["JwtOptions:Issuer"];
            var jwtAudience = builder.Configuration["JwtOptions:Audience"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,

                        ValidateAudience = true,
                        ValidAudience = jwtAudience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey!)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    jwtOptions.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrWhiteSpace(context.Token))
                            {
                                context.Token = context.Request.Cookies["accessToken"];
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(option =>
                {
                    option.AddDocument("v1", "API Version 1.0", "/openapi/v1.json", isDefault: true);

                });

                Console.WriteLine(myOptions.EnableIpRateLimiting);
                Console.WriteLine(myOptions.EnableClientIdRateLimiting);
                Console.WriteLine(myOptions.HttpStatusCode);
                Console.WriteLine(myOptions.RealIpHeader);
                Console.WriteLine(myOptions.ClientIdHeader);
                Console.WriteLine(myOptions.TimeoutInMinutes);
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
