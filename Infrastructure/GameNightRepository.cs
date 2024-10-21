using BordspelAvond.Application.Interfaces;
using BordspelAvond.Models;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Infrastructure
{
    public class GameNightRepository : IGameNightRepository
    {
        private readonly BordspelDbContext _context;

        public GameNightRepository(BordspelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameNight>> GetAllAsync()
        {
            return await _context.GameNights.ToListAsync();
        }

        public async Task<GameNight> GetByIdAsync(int id)
        {
            return await _context.GameNights.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<GameNight>> GetByOrganizerIdAsync(string organizerId)
        {
            return await _context.GameNights.Where(g => g.OrganizerId == organizerId).ToListAsync();
        }

        public async Task AddAsync(GameNight gameNight)
        {
            await _context.GameNights.AddAsync(gameNight);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GameNight gameNight)
        {
            _context.GameNights.Update(gameNight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var gameNight = await GetByIdAsync(id);
            if (gameNight != null)
            {
                _context.GameNights.Remove(gameNight);
                await _context.SaveChangesAsync();
            }
        }
    }
}
