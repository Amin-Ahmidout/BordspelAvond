using BordspelAvond.Application.Interfaces;
using BordspelAvond.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BordspelAvond.Presentation.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameNightApiController : ControllerBase
    {
        private readonly IGameNightService _gameNightService;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<GameNightApiController> _logger;

        public GameNightApiController(IGameNightService gameNightService, UserManager<Player> userManager, ILogger<GameNightApiController> logger)
        {
            _gameNightService = gameNightService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameNight>>> GetAllGameNights()
        {
            var gameNights = await _gameNightService.GetAllGameNightsAsync();
            return Ok(gameNights);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameNight>> GetGameNightById(int id)
        {
            var gameNight = await _gameNightService.GetGameNightDetailsAsync(id);
            if (gameNight == null)
            {
                _logger.LogWarning($"GameNight with id {id} not found");
                return NotFound("GameNight not found.");
            }
            return Ok(gameNight);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> CreateGameNight([FromBody] GameNight gameNight)
        {
            // Haal de huidige gebruiker op via het token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authorized to create a GameNight.");
            }

            // Stel het OrganizerId in op het userId van de ingelogde gebruiker
            gameNight.OrganizerId = userId;

            // Gebruik de service om de GameNight aan te maken
            var result = await _gameNightService.CreateGameNightAsync(gameNight, userId);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result);
        }







        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("{id}/join")]
        public async Task<ActionResult> JoinGameNight(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in.");
            }

            var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(id, userId);
            if (!success)
            {
                return BadRequest(errorMessage);
            }

            return Ok("Successfully joined the GameNight.");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("{id}/join-with-snack")]
        public async Task<ActionResult> JoinWithSnack(int id, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in.");
            }

            // Roep de service aan om deelname en snack toe te voegen
            var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(id, userId, snackName, isVegetarian, isLactoseFree, isNutFree, isAlcoholFree);

            if (!success)
            {
                return BadRequest(errorMessage);
            }

            return Ok("Successfully joined the GameNight with a snack.");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGameNight(int id)
        {
            // Haal het userId op uit de claims van de token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in.");
            }

            // Roep de service aan om GameNight te verwijderen
            var (success, errorMessage) = await _gameNightService.DeleteGameNightAsync(id, userId);
            if (!success)
            {
                return BadRequest(errorMessage);
            }

            return Ok("GameNight successfully deleted.");
        }


    }
}