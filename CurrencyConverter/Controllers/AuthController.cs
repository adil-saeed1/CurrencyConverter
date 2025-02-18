using Microsoft.AspNetCore.Mvc;
using CurrencyExchange.Application.Models;
using CurrencyExchange.Infrastructure.JWT;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest.ClientId == "admin" || loginRequest.ClientId == "guest")
            {
                var role = loginRequest.ClientId == "admin" ? "Admin" : "Guest";
                var token = new JWTTokenGenerator(_config).GenerateJwtToken(loginRequest.ClientId, role);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }
    }
}
