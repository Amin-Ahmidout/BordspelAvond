using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using BordspelAvond.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Presentation.Controllers
{
    public class GameNightController : Controller
    {
        private readonly BordspelDbContext _context;
        private readonly IGameNightService _gameNightService;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<GameNightController> _logger;

        public GameNightController(BordspelDbContext context, IGameNightService gameNightService, UserManager<Player> userManager, ILogger<GameNightController> logger)
        {
            _context = context;
            _gameNightService = gameNightService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: GameNight
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var gameNights = await _gameNightService.GetAllGameNightsAsync();
            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            return View(gameNights);
        }


        public async Task<IActionResult> MyOrganizedGames()
        {
            var organizerId = _userManager.GetUserId(User);
            if (organizerId == null)
            {
                var returnUrl = Url.Action("MyParticipatedGames", "GameNight");
                return Redirect($"/Identity/Account/Login?ReturnUrl={returnUrl}");
            }

            var organizedGames = await _gameNightService.GetGameNightsByOrganizerAsync(organizerId);
            return View(organizedGames);
        }

        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Roep de service aan om de businesslogica af te handelen, inclusief snacks
            var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(id, userId);

            if (!success)
            {
                TempData["JoinError"] = errorMessage;
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Details", new { id });
        }


        // Join met snack verplicht
        [HttpPost]
        public async Task<IActionResult> JoinWithSnack(int gameNightId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Roep de service aan om deelname en snack toe te voegen
            var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(gameNightId, userId, snackName, isVegetarian, isLactoseFree, isNutFree, isAlcoholFree);

            if (!success)
            {
                TempData["JoinError"] = errorMessage;
                return RedirectToAction("Details", new { id = gameNightId });
            }

            TempData["JoinSuccess"] = "Successfully joined the GameNight and added your snack!";
            return RedirectToAction("Details", new { id = gameNightId });
        }


        // GET: GameNight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null id");
                return NotFound();
            }

            var gameNight = await _gameNightService.GetGameNightDetailsAsync(id.Value);

            if (gameNight == null)
            {
                _logger.LogWarning($"GameNight with id {id} not found");
                return NotFound();
            }

            ViewBag.AvailableGames = await _gameNightService.GetAvailableGamesAsync();

            _logger.LogInformation($"Displaying details for GameNight with id {id}");
            return View(gameNight);
        }



        // GET: GameNight/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Create GET action called");
            return View();
        }

        // POST: GameNight/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Address,MaxPlayers,DateAndTime,Is18Plus,ProvidesVegetarianOptions,ProvidesLactoseFreeOptions,ProvidesNutsFreeOptions,ProvidesAlcoholFreeOptions,IsSnackRequired")] GameNight gameNight)
        {
            _logger.LogInformation("Create POST action called");

            ModelState.Remove("Organizer");

            if (ModelState.IsValid)
            {
                // Haal de organisator op
                var organizerId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(organizerId))
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve your account information. Please log in again.");
                    return View(gameNight);
                }

                var organizer = await _userManager.FindByIdAsync(organizerId);

                if (organizer != null)
                {
                    // Controleer of de organisator ouder is dan 18 jaar
                    var age = DateTime.Today.Year - organizer.BirthDate.Year;
                    if (organizer.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;

                    if (age < 18)
                    {
                        ModelState.AddModelError(string.Empty, "You must be at least 18 years old to organize a GameNight.");
                        return View(gameNight);
                    }

                    var (success, errorMessage) = await _gameNightService.CreateGameNightAsync(gameNight, organizerId);

                    if (success)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var safeErrorMessage = errorMessage ?? "An unexpected error occurred.";
                        ModelState.AddModelError(string.Empty, safeErrorMessage);
                        return View(gameNight);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "There was an issue retrieving your account details.");
                }
            }
            else
            {
                _logger.LogWarning($"ModelState is invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))}");
            }

            return View(gameNight);
        }




        // GET: GameNight/MyParticipatedGames
        public async Task<IActionResult> MyParticipatedGames()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                var returnUrl = Url.Action("MyParticipatedGames", "GameNight");
                return Redirect($"/Identity/Account/Login?ReturnUrl={returnUrl}");
            }

            var participatedGames = await _gameNightService.GetParticipatedGameNightsAsync(userId);

            return View(participatedGames);
        }

        // GET: GameNight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action called with null id");
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Edit action called without a logged-in user");
                return Unauthorized();
            }

            var gameNight = await _gameNightService.GetGameNightForEditAsync(id.Value, userId);

            if (gameNight == null)
            {
                return Forbid(); // Of NotFound, afhankelijk van wat je wilt teruggeven bij ongeldig toegang
            }

            return View(gameNight);
        }

        // POST: GameNight/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Address,MaxPlayers,DateAndTime,ProvidesVegetarianOptions,ProvidesLactoseFreeOptions,ProvidesNutsFreeOptions,ProvidesAlcoholFreeOptions")] GameNight gameNight)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Edit action called without a logged-in user");
                return Unauthorized();
            }

            var (success, errorMessage, existingGameNight) = await _gameNightService.EditGameNightAsync(id, gameNight, userId);

            if (!success)
            {
                var safeErrorMessage = errorMessage ?? "An unexpected error occurred.";
                ModelState.AddModelError(string.Empty, safeErrorMessage);
                return View(existingGameNight ?? gameNight);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: GameNight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action called with null id");
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Edit action called without a logged-in user");
                return Unauthorized();
            }

            var (success, errorMessage, gameNight) = await _gameNightService.GetGameNightForDeleteAsync(id.Value, userId);

            if (!success)
            {
                var safeErrorMessage = errorMessage ?? "An unexpected error occurred.";
                ModelState.AddModelError(string.Empty, safeErrorMessage);
                return View("Details", gameNight);
            }

            return View(gameNight);
        }

        // POST: GameNight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Edit action called without a logged-in user");
                return Unauthorized();
            }

            var (success, errorMessage) = await _gameNightService.DeleteGameNightAsync(id, userId);

            if (!success)
            {
                var safeErrorMessage = errorMessage ?? "An unexpected error occurred.";
                ModelState.AddModelError(string.Empty, safeErrorMessage);
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddGameToGameNight(int gameNightId, int gameId)
        {
            var (success, message) = await _gameNightService.AddGameToGameNightAsync(gameNightId, gameId);

            if (!success)
            {
                TempData["GameAddError"] = message;
            }
            else
            {
                TempData["GameAddSuccess"] = message;
            }

            return RedirectToAction("Details", new { id = gameNightId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSnackToGameNight(int gameNightId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree)
        {
            var (success, message) = await _gameNightService.AddSnackToGameNightAsync(gameNightId, snackName, isVegetarian, isLactoseFree, isNutFree, isAlcoholFree);

            if (!success)
            {
                TempData["SnackAddError"] = message;
            }
            else
            {
                TempData["SnackAddSuccess"] = "Snack added!";
            }

            return RedirectToAction("Details", new { id = gameNightId });
        }






        private async Task<bool> GameNightExists(int id)
        {
            return await _gameNightService.GameNightExistsAsync(id);
        }
    }
}
