using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Application.Payloads.Models;
using TaskManagement.Domain.Repository;

namespace TaskManagement.Application.JwtImplementation
{
    public class JwtImplementation
    {
        // Define a request model for user authentication
        public sealed record Query(string Username, string Password) : IRequest<string>;

        public class Handler : IRequestHandler<Query, string>
        {
            private readonly IRepository _repository;
            private readonly IConfiguration _configuration;

            public Handler(IRepository repository, IConfiguration configuration)
            {
                _repository = repository;
                _configuration = configuration;
            }

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate user
                var validateUserResponse = await _repository.validateuser(request.Username, request.Password);
                if (validateUserResponse == null)
                {
                    return "Invalide UserName or password";
                }
    
                // Load JWT settings from configuration
                var jwtSettings = new JwtSettings();
                _configuration.GetSection("JwtSettings").Bind(jwtSettings);

                var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("user", validateUserResponse.Username),
                        new Claim(ClaimTypes.Role, validateUserResponse.Role)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
                    Issuer = jwtSettings.Issuer,
                    Audience = jwtSettings.Audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
        }
    }
}
