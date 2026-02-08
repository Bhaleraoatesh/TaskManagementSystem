using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Payloads.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.IRepository;
using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<AuthenticationResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed - user not found: {Email}", request.Email);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Login attempt failed - invalid password for user: {UserId}", user.Id);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt failed - account is inactive: {UserId}", user.Id);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Account is inactive. Please contact support."
                    };
                }

                var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

                var (accessToken, jwtId, accessTokenExpiration) = await _jwtTokenService
                    .GenerateAccessTokenAsync(user, roles);

                var refreshToken = await _jwtTokenService
                    .GenerateRefreshTokenAsync(user.Id, jwtId, ipAddress);

                await _userRepository.UpdateLastLoginAsync(user.Id);

                _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

                return new AuthenticationResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    AccessTokenExpiration = accessTokenExpiration,
                    RefreshTokenExpiration = refreshToken.ExpiryDate,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = roles
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                };
            }
        }

        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            try
            {
                if (await _userRepository.UserExistsByEmailAsync(request.Email))
                {
                    _logger.LogWarning("Registration attempt failed - email already exists: {Email}", request.Email);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Email address is already registered"
                    };
                }

                // Split full name into first and last name
                var nameParts = request.FullName.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                var user = new User
                {
                    Email = request.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                var created = await _userRepository.CreateUserAsync(user);
                if (!created)
                {
                    _logger.LogError("Failed to create user during registration: {Email}", request.Email);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Failed to create user account. Please try again."
                    };
                }

                _logger.LogInformation("New user registered successfully: {UserId}, Email: {Email}", user.Id, user.Email);

                return new AuthenticationResponse
                {
                    Success = true,
                    Message = "Registration successful. You can now login.",
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = request.FullName,
                        Roles = new List<string>()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again."
                };
            }
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(
            RefreshTokenRequest request, 
            string ipAddress)
        {
            try
            {
                var principal = _jwtTokenService.ValidateAccessToken(request.AccessToken);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid access token provided for refresh");
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Invalid access token"
                    };
                }

                var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    _logger.LogWarning("JTI claim not found in access token");
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Invalid token claims"
                    };
                }

                var storedRefreshToken = await _jwtTokenService.VerifyRefreshTokenAsync(request.RefreshToken);
                if (storedRefreshToken == null)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                if (storedRefreshToken.JwtId != jti)
                {
                    _logger.LogWarning("Refresh token does not match access token. Possible token reuse attack.");
                    
                    await _jwtTokenService.RevokeRefreshTokenAsync(
                        storedRefreshToken, 
                        ipAddress, 
                        "Token mismatch - possible reuse attack");
                    
                    await _jwtTokenService.RevokeAllUserRefreshTokensAsync(
                        storedRefreshToken.UserId, 
                        ipAddress, 
                        "Security: Token mismatch detected");

                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "Security violation detected. All sessions have been terminated."
                    };
                }

                var userId = int.Parse(principal.Claims.First(c => c.Type == "userId").Value);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive during token refresh: {UserId}", userId);
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = "User account is not available"
                    };
                }

                var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

                var (newAccessToken, newJwtId, accessTokenExpiration) = await _jwtTokenService
                    .GenerateAccessTokenAsync(user, roles);
                
                var newRefreshToken = await _jwtTokenService
                    .GenerateRefreshTokenAsync(user.Id, newJwtId, ipAddress);

                await _jwtTokenService.RevokeRefreshTokenAsync(
                    storedRefreshToken,
                    ipAddress,
                    "Token used for refresh",
                    newRefreshToken.Token);

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

                return new AuthenticationResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    AccessTokenExpiration = accessTokenExpiration,
                    RefreshTokenExpiration = newRefreshToken.ExpiryDate,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = roles
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh. Please login again."
                };
            }
        }

        public async Task<ApiResponse> RevokeTokenAsync(RevokeTokenRequest request, string ipAddress)
        {
            try
            {
                var refreshToken = await _jwtTokenService.VerifyRefreshTokenAsync(request.RefreshToken);
                
                if (refreshToken == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                await _jwtTokenService.RevokeRefreshTokenAsync(
                    refreshToken, 
                    ipAddress, 
                    "Revoked by user");

                _logger.LogInformation("Token revoked for user: {UserId}", refreshToken.UserId);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Token revoked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while revoking the token"
                };
            }
        }

        public async Task<ApiResponse> LogoutAsync(int userId, string ipAddress)
        {
            try
            {
                await _jwtTokenService.RevokeAllUserRefreshTokensAsync(
                    userId, 
                    ipAddress, 
                    "User logout");

                _logger.LogInformation("User logged out: {UserId}", userId);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Logged out successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during logout"
                };
            }
        }
    }
}