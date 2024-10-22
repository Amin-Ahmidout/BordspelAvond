using BordspelAvond.Application.Interfaces;
using BordspelAvond.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

        public async Task<GameNight> GetByIdWithParticipantsAsync(int id)
        {
            return await _context.GameNights
                .Include(g => g.Participants)  // Include the participants in the query
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<GameNight> GetByIdWithDetailsAsync(int id)
        {
            return await _context.GameNights
                .Include(g => g.Participants)
                .Include(g => g.GameNightGames)
                    .ThenInclude(gng => gng.Game)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Game>> GetAvailableGamesAsync()
        {
            return await _context.Games.ToListAsync();
        }
        public async Task<IEnumerable<GameNight>> GetParticipatedGamesByUserAsync(string userId)
        {
            return await _context.GameNightParticipants
                .Include(p => p.GameNight)
                .Where(p => p.UserId == userId)
                .Select(p => p.GameNight)
                .ToListAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

    }
}
