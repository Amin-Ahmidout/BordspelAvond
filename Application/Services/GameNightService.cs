using BordspelAvond.Application.Interfaces;
using BordspelAvond.Models;

namespace BordspelAvond.Application.Services
{
    public class GameNightService : IGameNightService
    {
        private readonly IGameNightRepository _gameNightRepository;

        public GameNightService(IGameNightRepository gameNightRepository)
        {
            _gameNightRepository = gameNightRepository;
        }

        public async Task<IEnumerable<GameNight>> GetAllGameNightsAsync()
        {
            return await _gameNightRepository.GetAllAsync();
        }

        public async Task<GameNight> GetGameNightByIdAsync(int id)
        {
            return await _gameNightRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(string organizerId)
        {
            return await _gameNightRepository.GetByOrganizerIdAsync(organizerId);
        }

        public async Task AddGameNightAsync(GameNight gameNight)
        {
            await _gameNightRepository.AddAsync(gameNight);
        }

        public async Task UpdateGameNightAsync(GameNight gameNight)
        {
            await _gameNightRepository.UpdateAsync(gameNight);
        }

        public async Task DeleteGameNightAsync(int id)
        {
            await _gameNightRepository.DeleteAsync(id);
        }
    }
}
