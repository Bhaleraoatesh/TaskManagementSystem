using System.Security.Claims;
using TaskManagement.Application.Payloads.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface IJwtTokenService
    {
         Task<(string token, string jwtId, DateTime expiration)> GenerateAccessTokenAsync(User user, List<string> roles);
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string jwtId, string ipAddress);
        ClaimsPrincipal ValidateAccessToken(string token);
        Task<RefreshToken> VerifyRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null);
        Task RevokeAllUserRefreshTokensAsync(int userId, string ipAddress, string reason = null);
        Task CleanupExpiredTokensAsync();
    }
}