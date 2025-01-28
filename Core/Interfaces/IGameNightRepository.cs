using BordspelAvond.Core.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace BordspelAvond.Core.Interfaces
{
    public interface IGameNightRepository
    {
        Task<IEnumerable<GameNight>> GetAllAsync();
        Task<GameNight?> GetByIdAsync(int id);
        Task<IEnumerable<GameNight>> GetByOrganizerIdAsync(string organizerId);
        Task AddAsync(GameNight gameNight);
        Task UpdateAsync(GameNight gameNight);
        Task DeleteAsync(int id);
        Task<GameNight?> GetByIdWithParticipantsAsync(int id);
        Task<IEnumerable<Game>> GetAvailableGamesAsync();
        Task<GameNight?> GetByIdWithDetailsAsync(int id);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<IEnumerable<GameNight?>> GetParticipatedGamesByUserAsync(string userId);
        Task<Game?> GetGameByIdAsync(int id);
        Task<bool> GameNightExistsAsync(int id);

    }
}
