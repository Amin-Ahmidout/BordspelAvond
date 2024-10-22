using BordspelAvond.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BordspelAvond.Application.Interfaces
{
    public interface IGameNightService
    {
        Task<IEnumerable<GameNight>> GetAllGameNightsAsync();
        Task<GameNight> GetGameNightByIdAsync(int id);
        Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(string organizerId);
        Task AddGameNightAsync(GameNight gameNight);
        Task UpdateGameNightAsync(GameNight gameNight);
        Task DeleteGameNightAsync(int id);

        Task<(bool Success, string ErrorMessage)> JoinGameNightAsync(int gameNightId, string userId);
        Task<IEnumerable<Game>> GetAvailableGamesAsync();
        Task<GameNight> GetGameNightDetailsAsync(int id);
        Task<(bool Success, string ErrorMessage)> CreateGameNightAsync(GameNight gameNight, string userId);
    }
}
