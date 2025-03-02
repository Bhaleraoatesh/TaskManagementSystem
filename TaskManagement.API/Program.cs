using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using TaskManagement.Application.Payloads.Models;
using TaskManagement.Persistance.Extensions;
try
{
    // Set up Serilog before creating the builder
    Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
      .Enrich.FromLogContext()          
      .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);
   
    builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

    // Use Serilog for ASP.NET Core logging
    builder.Host.UseSerilog(); 

    Log.Information("TaskManagement.API is starting up.");

    // Load JWT settings
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
    builder.Services.AddSingleton(jwtSettings);

    // Register MediatR services
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TaskManagement.Application.Query.GetAssignedTasks.Handler).Assembly));

    // Register custom services
    builder.Services.AddServiceRegistration(builder.Configuration);

    // Add JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.OperationFilter<SecurityRequirementsOperationFilter>();
    });

    var app = builder.Build();

    var basePath = builder.Configuration.GetValue<string>("BasePath");
    var apiname = builder.Configuration.GetValue<string>("ApiName");
    var version = builder.Configuration.GetValue<string>("ApiVersion");

    app.UsePathBase(basePath);

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(basePath + $"/swagger/{version}/swagger.json", apiname);
    });

    app.UseAuthentication();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Information(ex.Message, "Unhandled exception occurred.");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush(); // Ensure all log entries are flushed when the app exits
}
