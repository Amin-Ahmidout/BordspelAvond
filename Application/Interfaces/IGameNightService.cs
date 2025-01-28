using BordspelAvond.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BordspelAvond.Application.Interfaces
{
    public interface IGameNightService
    {
        Task<IEnumerable<GameNight>> GetAllGameNightsAsync();
        Task<GameNight?> GetGameNightByIdAsync(int id);
        Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(string organizerId);
        Task AddGameNightAsync(GameNight gameNight);
        Task UpdateGameNightAsync(GameNight gameNight);
        Task DeleteGameNightAsync(int id);

        Task<(bool Success, string? ErrorMessage)> JoinGameNightAsync(int gameNightId, string userId);
        Task<(bool Success, string? ErrorMessage)> JoinGameNightAsync(int gameNightId, string userId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree);

        Task<IEnumerable<Game>> GetAvailableGamesAsync();
        Task<GameNight> GetGameNightDetailsAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateGameNightAsync(GameNight gameNight, string userId);
        Task<IEnumerable<GameNight?>> GetParticipatedGameNightsAsync(string userId);
        Task<(bool Success, string? ErrorMessage, GameNight? GameNight)> EditGameNightAsync(int id, GameNight gameNight, string userId);
        Task<GameNight?> GetGameNightForEditAsync(int id, string userId);
        Task<(bool Success, string? ErrorMessage, GameNight? GameNight)> GetGameNightForDeleteAsync(int id, string userId);
        Task<(bool Success, string? ErrorMessage)> DeleteGameNightAsync(int id, string userId);
        Task<(bool Success, string ErrorMessage)> AddGameToGameNightAsync(int gameNightId, int gameId);
        Task<bool> GameNightExistsAsync(int id);
        Task<(bool Success, string? ErrorMessage)> AddSnackToGameNightAsync(int gameNightId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree);

    }
}
