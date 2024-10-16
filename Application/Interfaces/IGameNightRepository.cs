using BordspelAvond.Models;

namespace BordspelAvond.Application.Interfaces
{
    public interface IGameNightRepository
    {
        Task<IEnumerable<GameNight>> GetAllAsync();
        Task<GameNight> GetByIdAsync(int id);
        Task<IEnumerable<GameNight>> GetByOrganizerIdAsync(string organizerId);
        Task AddAsync(GameNight gameNight);
        Task UpdateAsync(GameNight gameNight);
        Task DeleteAsync(int id);
    }
}
