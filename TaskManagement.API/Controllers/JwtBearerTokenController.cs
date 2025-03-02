using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Attributes;
using TaskManagement.Application.JwtImplementation;
using TaskManagement.Application.Payloads.Request;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Generate a JWT token
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="role">User's role</param>
        /// <returns>JWT token</returns>
        [HttpPost("jwtToken")]
        [ValidateModelState]
        public async Task<IActionResult> GenerateToken([FromBody] JsonPropertyName apiBody)
        {
            var token = await _mediator.Send(new JwtImplementation.Query(apiBody.Username!,apiBody.password!));
            if(token == "Invalide UserName or password") 
            { 
                return new ObjectResult(token)
                { 
                    StatusCode = 401 
                };     
            }
            return new ObjectResult(token)
            {
                StatusCode = 200
            };             
        }

    }
}
