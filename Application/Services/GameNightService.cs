using BordspelAvond.Application.Interfaces;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var gameNights = await _gameNightRepository.GetAllAsync();

            foreach (var gameNight in gameNights)
            {
                if (gameNight.OrganizerId != null)
                {
                    var organizer = await _userManager.FindByIdAsync(gameNight.OrganizerId);
                    if (organizer != null)
                    {
                        gameNight.Organizer = organizer;  
                    }
                }
            }

            return gameNights;
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

        public async Task<(bool Success, string ErrorMessage)> JoinGameNightAsync(int gameNightId, string userId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree)
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

            // Voeg deelnemer toe
            if (!gameNight.Participants.Any(p => p.UserId == userId))
            {
                var participant = new GameNightParticipant
                {
                    GameNightId = gameNight.Id,
                    UserId = userId
                };

                gameNight.Participants.Add(participant);
            }

            // Voeg snack toe als snacks vereist zijn
            if (!string.IsNullOrWhiteSpace(snackName))
            {
                var snack = new GameNightSnack
                {
                    GameNightId = gameNight.Id,
                    SnackName = snackName,
                    IsVegetarian = isVegetarian,
                    IsLactoseFree = isLactoseFree,
                    IsNutFree = isNutFree,
                    IsAlcoholFree = isAlcoholFree
                };

                gameNight.Snacks.Add(snack);
            }

            await _gameNightRepository.UpdateAsync(gameNight);

            return (true, null);
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





        public async Task<GameNight> GetGameNightDetailsAsync(int id)
        {
            var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);

            if (gameNight != null && gameNight.OrganizerId != null)
            {
                var organizer = await _userManager.FindByIdAsync(gameNight.OrganizerId);
                if (organizer != null)
                {
                    gameNight.Organizer = organizer;  // Voeg de organisator toe aan het model
                }
            }

            return gameNight;
        }


        public async Task<(bool Success, string ErrorMessage)> CreateGameNightAsync(GameNight gameNight, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return (false, "There is an issue with your account. Please log in again.");
            }

            gameNight.OrganizerId = userId;

            try
            {
                await _gameNightRepository.AddAsync(gameNight);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "An error occurred while saving the game night. Please try again.");
            }
        }
        public async Task<IEnumerable<GameNight>> GetParticipatedGameNightsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }

            var participatedGames = await _gameNightRepository.GetParticipatedGamesByUserAsync(userId);


            return participatedGames;
        }
        public async Task<(bool Success, string ErrorMessage, GameNight GameNight)> EditGameNightAsync(int id, GameNight gameNight, string userId)
        {
            var existingGameNight = await _gameNightRepository.GetByIdWithParticipantsAsync(id);
            if (existingGameNight == null)
            {
                return (false, "GameNight not found", null);
            }

            if (existingGameNight.OrganizerId != userId)
            {
                return (false, "User not authorized", null);
            }

            if (existingGameNight.Participants.Any())
            {
                return (false, "Cannot edit a Game Night with participants", existingGameNight);
            }

            existingGameNight.Address = gameNight.Address;
            existingGameNight.MaxPlayers = gameNight.MaxPlayers;
            existingGameNight.DateAndTime = gameNight.DateAndTime;
            existingGameNight.ProvidesVegetarianOptions = gameNight.ProvidesVegetarianOptions;
            existingGameNight.ProvidesLactoseFreeOptions = gameNight.ProvidesLactoseFreeOptions;
            existingGameNight.ProvidesNutsFreeOptions = gameNight.ProvidesNutsFreeOptions;
            existingGameNight.ProvidesAlcoholFreeOptions = gameNight.ProvidesAlcoholFreeOptions;

            try
            {
                await _gameNightRepository.UpdateAsync(existingGameNight);
                return (true, null, existingGameNight);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "An error occurred while updating the game night", existingGameNight);
            }
        }

        public async Task<GameNight> GetGameNightForEditAsync(int id, string userId)
        {
            var gameNight = await _gameNightRepository.GetByIdWithParticipantsAsync(id);
            if (gameNight == null || gameNight.OrganizerId != userId)
            {
                return null;
            }
            return gameNight;
        }
        public async Task<(bool Success, string ErrorMessage, GameNight GameNight)> GetGameNightForDeleteAsync(int id, string userId)
        {
            var gameNight = await _gameNightRepository.GetByIdWithParticipantsAsync(id);
            if (gameNight == null)
            {
                return (false, "GameNight not found", null);
            }

            if (gameNight.OrganizerId != userId)
            {
                return (false, "User not authorized", null);
            }

            if (gameNight.Participants.Any())
            {
                return (false, "Cannot delete a Game Night with participants", gameNight);
            }

            return (true, null, gameNight);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteGameNightAsync(int id, string userId)
        {
            var gameNight = await _gameNightRepository.GetByIdWithParticipantsAsync(id);
            if (gameNight == null)
            {
                return (false, "GameNight not found");
            }

            if (gameNight.OrganizerId != userId)
            {
                return (false, "User not authorized to delete this GameNight");
            }

            if (gameNight.Participants.Any())
            {
                return (false, "Cannot delete a Game Night with participants");
            }

            try
            {
                await _gameNightRepository.DeleteAsync(id);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "An error occurred while deleting the GameNight. Please try again.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddGameToGameNightAsync(int gameNightId, int gameId)
        {
            var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(gameNightId);
            if (gameNight == null)
            {
                return (false, "GameNight not found");
            }

            var game = await _gameNightRepository.GetGameByIdAsync(gameId);
            if (game == null)
            {
                return (false, "Game not found");
            }

            if (gameNight.GameNightGames.Any(gng => gng.GameId == gameId))
            {
                return (false, "Game is already added to this GameNight.");
            }

            gameNight.GameNightGames.Add(new GameNightGame { GameNightId = gameNightId, GameId = gameId });

            if (game.is18Plus && !gameNight.Is18Plus)
            {
                gameNight.Is18Plus = true;
            }

            await _gameNightRepository.UpdateAsync(gameNight);

            return (true, $"{game.Name} added to GameNight!");
        }


        public async Task<bool> GameNightExistsAsync(int id)
        {
            return await _gameNightRepository.GameNightExistsAsync(id);
        }

        public async Task<(bool Success, string ErrorMessage)> AddSnackToGameNightAsync(int gameNightId, string snackName, bool isVegetarian, bool isLactoseFree, bool isNutFree, bool isAlcoholFree)
        {
            var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(gameNightId);
            if (gameNight == null)
            {
                return (false, "GameNight not found.");
            }

            var snack = new GameNightSnack
            {
                GameNightId = gameNightId,
                SnackName = snackName,
                IsVegetarian = isVegetarian,
                IsLactoseFree = isLactoseFree,
                IsNutFree = isNutFree,
                IsAlcoholFree = isAlcoholFree
            };

            gameNight.Snacks.Add(snack);
            await _gameNightRepository.UpdateAsync(gameNight);

            return (true, null);
        }

        public async Task<IEnumerable<Game>> GetAvailableGamesAsync()
        {
            return await _gameNightRepository.GetAvailableGamesAsync();
        }
    }
}
