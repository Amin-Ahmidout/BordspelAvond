using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BordspelAvond.Models;
using System.Threading.Tasks;

namespace BordspelAvond.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Player> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<Player> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Email and password are required.");

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }

    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
