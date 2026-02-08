using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Payloads.DTOs;

namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authenticationService,
            ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var ipAddress = GetIpAddress();
            var result = await _authenticationService.LoginAsync(request, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var ipAddress = GetIpAddress();
            var result = await _authenticationService.RegisterAsync(request, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetCurrentUser), result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                request.RefreshToken = Request.Cookies["refreshToken"];
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var ipAddress = GetIpAddress();
            var result = await _authenticationService.RefreshTokenAsync(request, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var ipAddress = GetIpAddress();
            var result = await _authenticationService.LogoutAsync(userId, ipAddress);

            Response.Cookies.Delete("refreshToken");
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var fullName = User.FindFirst("fullName")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new { UserId = userId, Email = email, FullName = fullName, Roles = roles });
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}