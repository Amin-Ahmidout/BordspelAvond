using BordspelAvond.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Controllers
{
    public class GameController : Controller
    {
        private readonly BordspelDbContext _context;

        public GameController(BordspelDbContext context)
        {
            _context = context;
        }

        // GET: Game/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }
    }
}
