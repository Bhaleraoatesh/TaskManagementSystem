using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Attributes;
using TaskManagement.API.Helper.JwtTokenHelper;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Ijwthelper _jwtTokenHelper;

        public AuthController(Ijwthelper jwtTokenHelper)
        {
            _jwtTokenHelper = jwtTokenHelper;
        }

        /// <summary>
        /// Generate a JWT token
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="role">User's role</param>
        /// <returns>JWT token</returns>
        [HttpGet("jwtToken")]
        [ValidateModelState]
        public IActionResult GenerateToken([FromQuery] string username, [FromQuery] string role)
        {         
            var token = _jwtTokenHelper.GenerateToken(username, role);
            return Ok(new { token });
        }
    }
}
