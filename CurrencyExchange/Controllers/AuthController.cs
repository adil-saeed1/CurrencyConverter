using Microsoft.AspNetCore.Mvc;
using CurrencyExchange.Application.Models;
using CurrencyExchange.Infrastructure.JWT;
using StackExchange.Redis;

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
        public IActionResult Login([FromBody] LoginReq loginRequest)
        {
            var users = new Users().GetMockUsers();
            var user = users.FirstOrDefault(u => u.Username.Equals(loginRequest.UserName, StringComparison.OrdinalIgnoreCase) && u.Password == loginRequest.Password);

            if (user != null)
            {
                var clientId = Request.Headers["clientid"].FirstOrDefault();
                var token = new JWTTokenGenerator(_config).GenerateJwtToken(user.Username, clientId, user.Role);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }
    }
}
