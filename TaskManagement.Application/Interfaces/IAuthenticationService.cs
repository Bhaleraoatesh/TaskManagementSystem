using TaskManagement.Application.Payloads.DTOs;

namespace TaskManagement.Application.Interfaces
{
    /// <summary>
    /// Service for handling user authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
        Task<ApiResponse> RevokeTokenAsync(RevokeTokenRequest request, string ipAddress);
        Task<ApiResponse> LogoutAsync(int userId, string ipAddress);
    }
}