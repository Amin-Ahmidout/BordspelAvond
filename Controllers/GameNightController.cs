using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Controllers
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
            return View(gameNights);
        }


        public async Task<IActionResult> MyOrganizedGames()
        {
            var organizerId = _userManager.GetUserId(User);
            if (organizerId == null)
            {
                return RedirectToAction("Login", "Account");
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

            // Roep de service aan om de businesslogica af te handelen
            var (success, errorMessage) = await _gameNightService.JoinGameNightAsync(id, userId);

            if (!success)
            {
                TempData["JoinError"] = errorMessage;
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Details", new { id });
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
        public async Task<IActionResult> Create([Bind("Address,MaxPlayers,DateAndTime,Is18Plus,ProvidesVegetarianOptions,ProvidesLactoseFreeOptions,ProvidesNutsFreeOptions,ProvidesAlcoholFreeOptions")] GameNight gameNight)
        {
            _logger.LogInformation("Create POST action called");

            ModelState.Remove("Organizer");

            if (ModelState.IsValid)
            {
                var organizerId = _userManager.GetUserId(User);
                var (success, errorMessage) = await _gameNightService.CreateGameNightAsync(gameNight, organizerId);

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }
            else
            {
                _logger.LogWarning($"ModelState is invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))}");
            }

            return View(gameNight);
        }


        public async Task<IActionResult> MyParticipatedGames()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var participatedGames = await _context.GameNightParticipants
                .Include(p => p.GameNight)
                .Where(p => p.UserId == userId)
                .Select(p => p.GameNight)
                .ToListAsync();

            if (!participatedGames.Any())
            {
                _logger.LogInformation("No participated games found for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("{Count} participated games found for user {UserId}", participatedGames.Count, userId);
            }

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

            var gameNight = await _context.GameNights
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gameNight == null)
            {
                _logger.LogWarning($"GameNight with id {id} not found for edit");
                return NotFound();
            }

            var organizerId = _userManager.GetUserId(User);

            // Controleer of de gebruiker de organisator is en dat er geen deelnemers zijn
            if (gameNight.OrganizerId != organizerId)
            {
                _logger.LogWarning("User is not authorized to edit this GameNight");
                return Forbid(); // Gebruiker heeft geen toestemming om te bewerken
            }

            if (gameNight.Participants.Any())
            {
                ModelState.AddModelError(string.Empty, "Cannot edit a Game Night with participants.");
                return View("Details", gameNight); // Keer terug naar de Details-pagina met foutmelding
            }

            _logger.LogInformation($"Editing GameNight with id {id}");
            return View(gameNight);
        }


        // POST: GameNight/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Address,MaxPlayers,DateAndTime,ProvidesVegetarianOptions,ProvidesLactoseFreeOptions,ProvidesNutFreeOptions,ProvidesAlcoholFreeOptions")] GameNight gameNight)
        {
            ModelState.Remove("Organizer");

            if (id != gameNight.Id)
            {
                _logger.LogWarning($"Edit action called with mismatched id {id} for GameNight");
                return NotFound();
            }

            var existingGameNight = await _context.GameNights
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (existingGameNight == null)
            {
                return NotFound();
            }

            var organizerId = _userManager.GetUserId(User);

            if (existingGameNight.OrganizerId != organizerId)
            {
                return Forbid();
            }

            if (existingGameNight.Participants.Any())
            {
                ModelState.AddModelError(string.Empty, "Cannot edit a Game Night with participants.");
                return RedirectToAction(nameof(Details), new { id = gameNight.Id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingGameNight.Address = gameNight.Address;
                    existingGameNight.MaxPlayers = gameNight.MaxPlayers;
                    existingGameNight.DateAndTime = gameNight.DateAndTime;

                    // Update de dieetopties
                    existingGameNight.ProvidesVegetarianOptions = gameNight.ProvidesVegetarianOptions;
                    existingGameNight.ProvidesLactoseFreeOptions = gameNight.ProvidesLactoseFreeOptions;
                    existingGameNight.ProvidesNutsFreeOptions = gameNight.ProvidesNutsFreeOptions;
                    existingGameNight.ProvidesAlcoholFreeOptions = gameNight.ProvidesAlcoholFreeOptions;

                    _context.Update(existingGameNight);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"GameNight with id {gameNight.Id} successfully updated");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameNightExists(gameNight.Id))
                    {
                        _logger.LogWarning($"GameNight with id {gameNight.Id} not found during update");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogWarning($"ModelState is invalid during edit: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))}");
            }

            return View(gameNight);
        }


        // GET: GameNight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameNight = await _context.GameNights
                .Include(g => g.Participants) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gameNight == null)
            {
                return NotFound();
            }

            if (gameNight.Participants.Any())
            {
                ModelState.AddModelError(string.Empty, "You cannot delete this Game Night because there are participants.");
                return View("Details", gameNight); 
            }

            return View(gameNight);
        }

        // POST: GameNight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gameNight = await _context.GameNights
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gameNight == null)
            {
                return NotFound();
            }

            var organizerId = _userManager.GetUserId(User);

           
            if (gameNight.OrganizerId != organizerId)
            {
                return Forbid(); 
            }

            if (gameNight.Participants.Any())
            {
                ModelState.AddModelError(string.Empty, "Cannot delete a Game Night with participants.");
                return RedirectToAction(nameof(Details), new { id = gameNight.Id });
            }

            _context.GameNights.Remove(gameNight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddGameToGameNight(int gameNightId, int gameId)
        {
            var gameNight = await _context.GameNights
                .Include(g => g.GameNightGames)
                .FirstOrDefaultAsync(g => g.Id == gameNightId);
            var game = await _context.Games.FindAsync(gameId);

            if (gameNight == null || game == null)
            {
                return NotFound();
            }

         
            if (gameNight.GameNightGames.Any(gng => gng.GameId == gameId))
            {
                TempData["GameAddError"] = "Game is already added to this GameNight.";
            }
            else
            {
                gameNight.GameNightGames.Add(new GameNightGame { GameNightId = gameNightId, GameId = gameId });
                await _context.SaveChangesAsync();
                TempData["GameAddSuccess"] = $"{game.Name} added to GameNight!";
            }

            return RedirectToAction("Details", new { id = gameNightId });
        }




        private bool GameNightExists(int id)
        {
            return _context.GameNights.Any(e => e.Id == id);
        }
    }
}
