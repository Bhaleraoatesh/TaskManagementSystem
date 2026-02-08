using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TaskManagement.API.BackgroundServices;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Payloads.Models;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Persistance.Repositories;
using TaskManagement.Domain.IRepository;
using TaskManagement.Persistance.Extensions;
using System.Text.Json;
try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    builder.Host.UseSerilog();

    Log.Information("TaskManagement.API is starting up.");

    // Load JWT settings
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
    Log.Information("jwt settings loaded: {@JwtSettings}", JsonSerializer.Serialize(jwtSettings));
    if (string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key.Length < 32)
    {
        throw new InvalidOperationException("JWT Key must be at least 32 characters");
    }
    
    builder.Services.AddSingleton(jwtSettings);

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
        
        options.AddPolicy("Production",
            policy => policy.WithOrigins(
                    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                    ?? new[] { "https://yourdomain.com" })
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
    });

    

    // Register Services
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddServiceRegistration(builder.Configuration); // Infrastructure

    // Add Background Services
    builder.Services.AddHostedService<TokenCleanupService>();

    // Add JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                    Log.Warning("Token expired");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("userId")?.Value;
                Log.Debug("Token validated for user: {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });

    // Add Authorization policies
    //builder.Services.AddAuthorization(options =>
    //{
   //     options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
     //   options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Admin", "Manager"));
   // });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Task Management API",
            Version = "v1",
            Description = "JWT Authentication with Refresh Tokens using Dapper"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using Bearer scheme. Enter 'Bearer' [space] and your token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });
    });

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    var basePath = builder.Configuration.GetValue<string>("BasePath") ?? "";

    if (!string.IsNullOrEmpty(basePath))
    {
        app.UsePathBase(basePath);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseCors("AllowAll");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"{basePath}/swagger/v1/swagger.json", "Task Management API v1");
        });
    }
    else
    {
        app.UseCors("Production");
    }

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");
        
        await next();
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("TaskManagement.API started successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}