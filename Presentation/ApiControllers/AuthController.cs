using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BordspelAvond.Core.Domain;
using System.ComponentModel.DataAnnotations;

namespace BordspelAvond.Presentation.ApiControllers
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Validatie van de request body
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Controleer of de gebruiker al bestaat
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Conflict(new { message = "Email is already registered." });

            // Bereken leeftijd
            var today = DateTime.Today;
            var age = today.Year - model.BirthDate.Year;
            if (model.BirthDate.Date > today.AddYears(-age)) age--;

            if (age < 16)
                return BadRequest(new { message = "You must be at least 16 years old to register." });

            // Maak een nieuwe gebruiker aan
            var user = new Player
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                BirthDate = model.BirthDate,
                Gender = model.Gender,
                City = model.City,
                Street = model.Street,
                HouseNumber = model.HouseNumber,
                IsVegetarian = model.IsVegetarian,
                HasLactoseAllergy = model.HasLactoseAllergy,
                HasNutsAllergy = model.HasNutsAllergy,
                AvoidsAlcohol = model.AvoidsAlcohol
            };

            // Maak de gebruiker aan met een wachtwoord
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = "Registration failed.", errors = result.Errors });

            return Ok(new
            {
                message = "Registration successful!",
                userId = user.Id
            });
        }

    }

    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime BirthDate { get; set; }

        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public bool IsVegetarian { get; set; }
        public bool HasLactoseAllergy { get; set; }
        public bool HasNutsAllergy { get; set; }
        public bool AvoidsAlcohol { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

}
