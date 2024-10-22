using BordspelAvond.Application.Interfaces;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BordspelAvond.Application.Services
{
    public class GameNightService : IGameNightService
    {
        private readonly IGameNightRepository _gameNightRepository;
        private readonly UserManager<Player> _userManager;



        public GameNightService(IGameNightRepository gameNightRepository, UserManager<Player> userManager)
        {
            _gameNightRepository = gameNightRepository;
            _userManager = userManager;
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

        public async Task<(bool Success, string ErrorMessage)> JoinGameNightAsync(int gameNightId, string userId)
        {
            var gameNight = await _gameNightRepository.GetByIdWithParticipantsAsync(gameNightId);
            if (gameNight == null)
            {
                return (false, "GameNight not found.");
            }

            // Controleer of het maximaal aantal spelers is bereikt
            if (gameNight.Participants.Count >= gameNight.MaxPlayers)
            {
                return (false, "This GameNight is full. No more players can join.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            // Controleer de leeftijdsrestrictie
            if (gameNight.Is18Plus)
            {
                var age = DateTime.Today.Year - user.BirthDate.Year;
                if (user.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;
                if (age < 18)
                {
                    return (false, "You must be at least 18 years old to join this event.");
                }
            }

            // Controleer dieetwensen tegen de beschikbare opties op de GameNight
            if ((user.IsVegetarian && !gameNight.ProvidesVegetarianOptions) ||
                (user.HasLactoseAllergy && !gameNight.ProvidesLactoseFreeOptions) ||
                (user.HasNutsAllergy && !gameNight.ProvidesNutsFreeOptions) ||
                (user.AvoidsAlcohol && !gameNight.ProvidesAlcoholFreeOptions))
            {
                return (false, "The available food and drink options do not meet your dietary requirements.");
            }

            // Controleer of de gebruiker al deelneemt
            if (!gameNight.Participants.Any(p => p.UserId == userId))
            {
                var participant = new GameNightParticipant
                {
                    GameNightId = gameNight.Id,
                    UserId = userId
                };

                gameNight.Participants.Add(participant);
                await _gameNightRepository.UpdateAsync(gameNight);
            }

            return (true, null);
        }







        public Task<IEnumerable<Game>> GetAvailableGamesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
