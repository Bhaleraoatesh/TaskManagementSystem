using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Payloads.Models;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.IRepository;
namespace TaskManagement.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            JwtSettings jwtSettings,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<(string token, string jwtId, DateTime expiration)> GenerateAccessTokenAsync(
            User user, 
            List<string> roles)
        {
            var jwtId = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("fullName", user.FullName ?? string.Empty)
            };

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("Access token generated for user {UserId} with JTI {JwtId}", 
                user.Id, jwtId);

            return (tokenString, jwtId, expiration);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(
            int userId, 
            string jwtId, 
            string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureRandomToken(),
                JwtId = jwtId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedByIp = ipAddress,
                IsUsed = false,
                IsRevoked = false
            };

            refreshToken.Id = await _refreshTokenRepository.InsertAsync(refreshToken);

            _logger.LogInformation("Refresh token created for user {UserId}", userId);

            return refreshToken;
        }

        public ClaimsPrincipal ValidateAccessToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm");
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogDebug("Token is expired");
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var claims = jwtToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims);
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return null;
            }
        }

        public async Task<RefreshToken> VerifyRefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found");
                return null;
            }

            if (!refreshToken.IsActive)
            {
                _logger.LogWarning("Refresh token is not active. UserId: {UserId}", refreshToken.UserId);
                return null;
            }

            return refreshToken;
        }

        public async Task RevokeRefreshTokenAsync(
            RefreshToken token, 
            string ipAddress, 
            string reason = null, 
            string replacedByToken = null)
        {
            await _refreshTokenRepository.RevokeAsync(token.Token, ipAddress, reason, replacedByToken);

            _logger.LogInformation("Refresh token revoked for user {UserId}. Reason: {Reason}", 
                token.UserId, reason ?? "Not specified");
        }

        public async Task RevokeAllUserRefreshTokensAsync(
            int userId, 
            string ipAddress, 
            string reason = null)
        {
            var count = await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, ipAddress, reason);

            _logger.LogInformation("All refresh tokens revoked for user {UserId}. Count: {Count}", 
                userId, count);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var count = await _refreshTokenRepository.CleanupExpiredTokensAsync();

            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", count);
        }

        private string GenerateSecureRandomToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}