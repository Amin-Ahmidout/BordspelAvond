using BordspelAvond.Application.Interfaces;
using BordspelAvond.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
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

        [HttpGet("organized")]
        [Authorize(AuthenticationSchemes = "Bearer")] // Zorgt ervoor dat alleen geauthenticeerde gebruikers toegang hebben
        public async Task<ActionResult<IEnumerable<GameNight>>> GetOrganizedGameNights()
        {
            // Haal de userId of email op uit de token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved userId from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No valid user ID found in token.");
                return Unauthorized(new { Message = "User not logged in or unauthorized." });
            }

            // Zoek de gebruiker op met FindByEmailAsync omdat userId een e-mailadres is
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User with email {userId} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            // Gebruik de service om de georganiseerde games op te halen
            var organizedGames = await _gameNightService.GetGameNightsByOrganizerAsync(user.Id);
            _logger.LogInformation($"Organized games count: {organizedGames.Count()}");

            return Ok(organizedGames);
        }

        [HttpGet("participated")]
        [Authorize(AuthenticationSchemes = "Bearer")] // Alleen geauthenticeerde gebruikers
        public async Task<ActionResult<IEnumerable<GameNight>>> GetParticipatedGames()
        {
            // Haal de userId of email op uit de token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved userId from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No valid user ID found in token.");
                return Unauthorized(new { Message = "User not logged in or unauthorized." });
            }

            // Zoek de gebruiker op via email
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User with email {userId} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            // Gebruik de service om deelgenomen spelavonden op te halen
            var participatedGames = await _gameNightService.GetParticipatedGameNightsAsync(user.Id);
            _logger.LogInformation($"Participated games count: {participatedGames.Count()}");

            return Ok(participatedGames);
        }



        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> CreateGameNight([Bind("Address,MaxPlayers,DateAndTime,Is18Plus,ProvidesVegetarianOptions,ProvidesLactoseFreeOptions,ProvidesNutsFreeOptions,ProvidesAlcoholFreeOptions,IsSnackRequired")] GameNight gameNight)
        {
            // Log start van de actie
            _logger.LogInformation("Start CreateGameNight action");

            // Haal de userId of email op uit de token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            _logger.LogInformation($"Retrieved userId from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No valid user ID found in token.");
                return Unauthorized(new { Message = "User not authorized to create a GameNight. Please log in." });
            }

            // Zoek de gebruiker op met FindByEmailAsync omdat userId een e-mailadres is
            var organizer = await _userManager.FindByEmailAsync(userId);
            if (organizer == null)
            {
                _logger.LogError($"User with email {userId} not found in the database.");
                return BadRequest(new { Message = "Organizer not found for the logged-in user." });
            }

            _logger.LogInformation($"Organizer found: {organizer.UserName}");

            // Stel de OrganizerId en Organizer in
            gameNight.OrganizerId = organizer.Id;
            gameNight.Organizer = organizer;

            _logger.LogInformation($"OrganizerId set to {gameNight.OrganizerId} for the new GameNight.");

            // Controleer of het model geldig is
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning($"Model validation failed: {string.Join(", ", validationErrors)}");
                return BadRequest(new
                {
                    Message = "Validation failed for the request.",
                    Errors = validationErrors
                });
            }

            _logger.LogInformation($"GameNight model is valid. Proceeding to create GameNight.");

            // Sla de GameNight op via de service
            var result = await _gameNightService.CreateGameNightAsync(gameNight, organizer.Id);
            if (!result.Success)
            {
                _logger.LogError($"Failed to create GameNight. Error: {result.ErrorMessage}");
                return BadRequest(new
                {
                    Message = "Failed to create GameNight.",
                    Error = result.ErrorMessage
                });
            }

            _logger.LogInformation($"GameNight with ID {gameNight.Id} created successfully by user {organizer.UserName}.");

            // Geef een succesvolle response terug
            return Ok(new
            {
                Message = "GameNight created successfully.",
                GameNightId = gameNight.Id,
                OrganizerId = gameNight.OrganizerId,
                Organizer = new
                {
                    Name = organizer.UserName,
                    Gender = organizer.Gender,
                    Email = organizer.Email,
                    BirthDate = organizer.BirthDate,
                    Street = organizer.Street,
                    HouseNumber = organizer.HouseNumber,
                    City = organizer.City
                }
            });
        }


        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinGameNight(int id)
        {
            // Log start van de actie
            _logger.LogInformation($"JoinGameNight API called with GameNight ID: {id}");

            // Haal de userId of email op uit de claims
            var userIdOrEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved userIdOrEmail from token: {userIdOrEmail}");

            if (string.IsNullOrEmpty(userIdOrEmail))
            {
                _logger.LogWarning("No valid user ID or email found in token.");
                return Unauthorized(new { Message = "User not logged in." });
            }

            // Zoek gebruiker op via e-mail
            var user = await _userManager.FindByEmailAsync(userIdOrEmail);
            if (user == null)
            {
                _logger.LogWarning($"User with email {userIdOrEmail} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            // Roep de service aan om de GameNight te joinen
            try
            {
                var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(id, user.Id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to join GameNight. Error: {errorMessage}");
                    return BadRequest(new { Message = errorMessage });
                }

                _logger.LogInformation($"User {user.Id} successfully joined GameNight {id}.");
                return Ok(new { Message = "Successfully joined the GameNight." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while user {user.Id} tried to join GameNight {id}.");
                return StatusCode(500, new { Message = "An unexpected error occurred. Please try again later." });
            }
        }







        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("{id}/join-with-snack")]
        public async Task<ActionResult> JoinWithSnack(int id, [FromBody] JsonElement body)
        {
            // Log start van de actie
            _logger.LogInformation($"JoinWithSnack API called with GameNight ID: {id}");

            // Haal de userId op uit de token
            var userIdOrEmail = User.FindFirstValue(ClaimTypes.NameIdentifier); // ID of email
            _logger.LogInformation($"Retrieved userIdOrEmail from token: {userIdOrEmail}");

            if (string.IsNullOrEmpty(userIdOrEmail))
            {
                _logger.LogWarning("No valid user ID or email found in token.");
                return Unauthorized(new { Message = "User not logged in." });
            }

            // Probeer gebruiker te vinden via ID
            var user = await _userManager.FindByIdAsync(userIdOrEmail);

            // Als de gebruiker niet wordt gevonden, probeer dan via email
            if (user == null)
            {
                _logger.LogInformation($"User not found by ID {userIdOrEmail}, trying by email.");
                user = await _userManager.FindByEmailAsync(userIdOrEmail);
            }

            // Controleer of de gebruiker bestaat
            if (user == null)
            {
                _logger.LogWarning($"User with ID or email {userIdOrEmail} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            try
            {
                // Parse de body handmatig
                if (!body.TryGetProperty("SnackName", out JsonElement snackNameElement) ||
                    !body.TryGetProperty("IsVegetarian", out JsonElement isVegetarianElement) ||
                    !body.TryGetProperty("IsLactoseFree", out JsonElement isLactoseFreeElement) ||
                    !body.TryGetProperty("IsNutFree", out JsonElement isNutFreeElement) ||
                    !body.TryGetProperty("IsAlcoholFree", out JsonElement isAlcoholFreeElement))
                {
                    _logger.LogWarning("One or more required fields are missing in the request body.");
                    return BadRequest(new { Message = "Invalid request body. Please provide all required fields." });
                }


                string snackName = snackNameElement.GetString()!;
                bool isVegetarian = isVegetarianElement.GetBoolean();
                bool isLactoseFree = isLactoseFreeElement.GetBoolean();
                bool isNutFree = isNutFreeElement.GetBoolean();
                bool isAlcoholFree = isAlcoholFreeElement.GetBoolean();

                _logger.LogInformation($"Snack Details: Name={snackName}, Vegetarian={isVegetarian}, LactoseFree={isLactoseFree}, NutFree={isNutFree}, AlcoholFree={isAlcoholFree}");

                // Roep de service aan om deelname en snack toe te voegen
                var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(
                    id,
                    user.Id, // Gebruik het correcte GUID-ID
                    snackName,
                    isVegetarian,
                    isLactoseFree,
                    isNutFree,
                    isAlcoholFree
                );

                if (!success)
                {
                    _logger.LogWarning($"Failed to join GameNight. Error: {errorMessage}");
                    return BadRequest(new { Message = errorMessage });
                }

                _logger.LogInformation($"User {user.Id} successfully joined GameNight {id} with a snack.");
                return Ok(new { Message = "Successfully joined the GameNight with a snack." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Required properties are missing in the request body.");
                return BadRequest(new { Message = "Invalid request body. Please provide all required fields." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while user {user.Id} tried to join GameNight {id}.");
                return StatusCode(500, new { Message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("{id}/edit")]
        public async Task<IActionResult> EditGameNight(int id, [FromBody] GameNight updatedGameNight)
        {
            // Log start van de actie
            _logger.LogInformation($"EditGameNight API called with GameNight ID: {id}");

            // Haal de userId of email op uit de claims
            var userIdOrEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved userIdOrEmail from token: {userIdOrEmail}");

            if (string.IsNullOrEmpty(userIdOrEmail))
            {
                _logger.LogWarning("No valid user ID or email found in token.");
                return Unauthorized(new { Message = "User not logged in." });
            }

            // Zoek gebruiker op via e-mail
            var user = await _userManager.FindByEmailAsync(userIdOrEmail);
            if (user == null)
            {
                _logger.LogWarning($"User with email {userIdOrEmail} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            // Roep de service aan om de GameNight te bewerken
            try
            {
                var (success, errorMessage, existingGameNight) = await _gameNightService.EditGameNightAsync(id, updatedGameNight, user.Id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to edit GameNight ID: {id}. Error: {errorMessage}");
                    return BadRequest(new { Message = errorMessage });
                }

                _logger.LogInformation($"GameNight {id} successfully edited by user {user.Id}.");
                return Ok(new
                {
                    Message = "GameNight edited successfully.",
                    GameNightId = existingGameNight!.Id,
                    Address = existingGameNight.Address,
                    MaxPlayers = existingGameNight.MaxPlayers,
                    DateAndTime = existingGameNight.DateAndTime,
                    Organizer = new
                    {
                        Name = user.UserName,
                        Email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while user {user.Id} tried to edit GameNight {id}.");
                return StatusCode(500, new { Message = "An unexpected error occurred. Please try again later." });
            }
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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("{gameNightId}/add-game")]
        public async Task<IActionResult> AddGameToGameNight(int gameNightId, [FromBody] int gameId)
        {
            // Log start van de actie
            _logger.LogInformation($"AddGameToGameNight API called for GameNight ID: {gameNightId} with Game ID: {gameId}");

            // Haal de userId of email op uit de claims
            var userIdOrEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved userIdOrEmail from token: {userIdOrEmail}");

            if (string.IsNullOrEmpty(userIdOrEmail))
            {
                _logger.LogWarning("No valid user ID or email found in token.");
                return Unauthorized(new { Message = "User not logged in." });
            }

            // Zoek gebruiker op via e-mail
            var user = await _userManager.FindByEmailAsync(userIdOrEmail);
            if (user == null)
            {
                _logger.LogWarning($"User with email {userIdOrEmail} not found in the database.");
                return BadRequest(new { Message = "User not found in the database." });
            }

            _logger.LogInformation($"User found: {user.UserName}");

            var gameNight = await _gameNightService.GetGameNightByIdAsync(gameNightId);
            if (gameNight == null)
            {
                return BadRequest(new { Message = "GameNight not found" });
            }

            if (gameNight.OrganizerId != user.Id)
            {
                return Forbid("Bearer");
            }

            // Roep de service aan om het spel toe te voegen aan de GameNight
            try
            {
                var (success, message) = await _gameNightService.AddGameToGameNightAsync(gameNightId, gameId);
                if (!success)
                {
                    _logger.LogWarning($"Failed to add Game ID: {gameId} to GameNight ID: {gameNightId}. Error: {message}");
                    return BadRequest(new { Message = message });
                }

                _logger.LogInformation($"Game ID: {gameId} successfully added to GameNight ID: {gameNightId} by user {user.Id}.");
                return Ok(new { Message = "Game added to GameNight successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while adding Game ID: {gameId} to GameNight ID: {gameNightId} by user {user.Id}.");
                return StatusCode(500, new { Message = "An unexpected error occurred. Please try again later." });
            }
        }


    }
}
