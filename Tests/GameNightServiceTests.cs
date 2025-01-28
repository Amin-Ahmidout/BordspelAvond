using System.Threading.Tasks;
using Xunit;
using Moq;
using BordspelAvond.Application.Services;
using Microsoft.AspNetCore.Identity;
using BordspelAvond.Core.Domain;
using BordspelAvond.Core.Interfaces;

namespace BordspelAvond.Tests
{
    public class GameNightServiceTests
    {
        private readonly Mock<IGameNightRepository> _mockRepository;
        private readonly GameNightService _service;
        private readonly Mock<UserManager<Player>> _mockUserManager;

        public GameNightServiceTests()
        {
            // Mock setup voor de repository
            _mockRepository = new Mock<IGameNightRepository>();
            _mockUserManager = new Mock<UserManager<Player>>(Mock.Of<IUserStore<Player>>(), null!, null!, null!, null!, null!, null!, null!, null!);

            // Instantieer de service met de mock
            _service = new GameNightService(_mockRepository.Object, _mockUserManager.Object);
        }

        //-----------------Test voor de CreateGameNightAsync-methode------------------------------------------------->
        [Fact]
        public async Task CreateGameNightAsync_ValidGameNight_ReturnsSuccess()
        {
            // Arrange: Maak een geldige GameNight en userId aan
            var gameNight = new GameNight { Address = "123 Board Game St", MaxPlayers = 10, OrganizerId = "user123" };
            var userId = "user123";

            // Stel in dat AddAsync succesvol voltooit
            _mockRepository.Setup(repo => repo.AddAsync(gameNight)).Returns(Task.CompletedTask);

            // Act: Roep de CreateGameNightAsync-methode aan
            var (success, errorMessage) = await _service.CreateGameNightAsync(gameNight, userId);

            // Assert: Controleer of het resultaat succesvol was en dat AddAsync precies één keer werd aangeroepen
            Assert.True(success);
            Assert.Null(errorMessage);
            _mockRepository.Verify(repo => repo.AddAsync(gameNight), Times.Once);
        }

        [Fact]
        public async Task CreateGameNightAsync_InvalidUserId_ReturnsError()
        {
            // Arrange: Maak een geldige GameNight aan, maar gebruik een lege userId
            var gameNight = new GameNight { Address = "123 Board Game St", MaxPlayers = 10 };
            var userId = ""; // Ongeldige userId

            // Act: Roep de CreateGameNightAsync-methode aan
            var (success, errorMessage) = await _service.CreateGameNightAsync(gameNight, userId);

            // Assert: Controleer of het resultaat niet succesvol was en dat er een foutmelding is
            Assert.False(success);
            Assert.Equal("There is an issue with your account. Please log in again.", errorMessage);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<GameNight>()), Times.Never);
        }

        //----------Test voor een succesvolle JoinGameNightAsync---------------------------------------->
        [Fact]
        public async Task JoinGameNightAsync_ValidUser_CanJoinSuccessfully()
        {
            // Arrange
            var gameNight = new GameNight
            {
                Id = 1,
                MaxPlayers = 5,
                Participants = new List<GameNightParticipant>(),
                Is18Plus = false
            };

            var user = new Player { Id = "user123", BirthDate = new System.DateTime(2000, 1, 1) };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNight.Id)).ReturnsAsync(gameNight);
            _mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user); // Correcte mock opzetten

            // Act
            var (success, errorMessage) = await _service.JoinGameNightAsync(gameNight.Id, user.Id);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(gameNight), Times.Once);
        }


        //----------Test voor een fout wanneer het maximaal aantal spelers is bereikt------------------->
        [Fact]
        public async Task JoinGameNightAsync_MaxPlayersReached_ReturnsError()
        {
            // Arrange
            var gameNight = new GameNight
            {
                Id = 1,
                MaxPlayers = 1,
                Participants = new List<GameNightParticipant> { new GameNightParticipant { GameNightId = 1, UserId = "user456" } },
                Is18Plus = false
            };
            var user = new Player { Id = "user123", BirthDate = new System.DateTime(2000, 1, 1) };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNight.Id)).ReturnsAsync(gameNight);
            _mockUserManager.Setup(um => um.FindByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            var (success, errorMessage) = await _service.JoinGameNightAsync(gameNight.Id, user.Id);

            // Assert
            Assert.False(success);
            Assert.Equal("This GameNight is full. No more players can join.", errorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(gameNight), Times.Never);
        }
        //----------Test voor een succesvolle edit------------------->
        [Fact]
        public async Task EditGameNightAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user123";
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = userId,
                Participants = new List<GameNightParticipant>(), // Geen deelnemers
                Address = "Old Address",
                MaxPlayers = 5
            };
            var updatedGameNight = new GameNight
            {
                Id = gameNightId,
                Address = "New Address", // De geüpdatete waarden
                MaxPlayers = 10
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage, updatedResult) = await _service.EditGameNightAsync(gameNightId, updatedGameNight, userId);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.Equal("New Address", updatedResult?.Address); // Controleer of het adres is bijgewerkt
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Once);
        }

        //----------FOutieve edit tests------------------->
        [Fact]
        public async Task EditGameNightAsync_UserNotAuthorized_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user456"; // Niet de organisator
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = "user123", // Andere gebruiker is de organisator
                Participants = new List<GameNightParticipant>() // Geen deelnemers
            };
            var updatedGameNight = new GameNight
            {
                Id = gameNightId,
                Address = "New Address",
                MaxPlayers = 10
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage, updatedResult) = await _service.EditGameNightAsync(gameNightId, updatedGameNight, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("User not authorized", errorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Never);
        }

        [Fact]
        public async Task EditGameNightAsync_GameNightHasParticipants_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user123";
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = userId,
                Participants = new List<GameNightParticipant> { new GameNightParticipant { GameNightId = 1, UserId = "participant1" } } // Heeft deelnemers
            };
            var updatedGameNight = new GameNight
            {
                Id = gameNightId,
                Address = "New Address",
                MaxPlayers = 10
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage, updatedResult) = await _service.EditGameNightAsync(gameNightId, updatedGameNight, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("Cannot edit a Game Night with participants", errorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Never);
        }

        //----------Test voor een succesvolle delete------------------->
        [Fact]
        public async Task DeleteGameNightAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user123";
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = userId,
                Participants = new List<GameNightParticipant>() // Geen deelnemers
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage) = await _service.DeleteGameNightAsync(gameNightId, userId);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            _mockRepository.Verify(repo => repo.DeleteAsync(gameNightId), Times.Once);
        }

        //----------Foutieve delete tests------------------->
        [Fact]
        public async Task DeleteGameNightAsync_UserNotAuthorized_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user456"; // Niet de organisator
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = "user123", // Andere gebruiker is de organisator
                Participants = new List<GameNightParticipant>() // Geen deelnemers
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage) = await _service.DeleteGameNightAsync(gameNightId, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("User not authorized to delete this GameNight", errorMessage);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteGameNightAsync_GameNightHasParticipants_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var userId = "user123";
            var gameNight = new GameNight
            {
                Id = gameNightId,
                OrganizerId = userId,
                Participants = new List<GameNightParticipant> { new GameNightParticipant { GameNightId = 1, UserId = "participant1" } } // Heeft deelnemers
            };

            _mockRepository.Setup(repo => repo.GetByIdWithParticipantsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage) = await _service.DeleteGameNightAsync(gameNightId, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("Cannot delete a Game Night with participants", errorMessage);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        //----------Test voor een succesvolle AddSnackToGameNightAsync------------------->
        [Fact]
        public async Task AddSnackToGameNightAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var gameNightId = 1;
            var gameNight = new GameNight
            {
                Id = gameNightId,
                Snacks = new List<GameNightSnack>()
            };

            var snackName = "Chips";
            var isVegetarian = true;
            var isLactoseFree = true;
            var isNutFree = true;
            var isAlcoholFree = true;

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync(gameNight);

            // Act
            var (success, errorMessage) = await _service.AddSnackToGameNightAsync(gameNightId, snackName, isVegetarian, isLactoseFree, isNutFree, isAlcoholFree);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.Single(gameNight.Snacks); // Controleer of er één snack is toegevoegd
            Assert.Equal(snackName, gameNight.Snacks.First().SnackName);
            Assert.True(gameNight.Snacks.First().IsVegetarian);
            Assert.True(gameNight.Snacks.First().IsLactoseFree);
            Assert.True(gameNight.Snacks.First().IsNutFree);
            Assert.True(gameNight.Snacks.First().IsAlcoholFree);
            _mockRepository.Verify(repo => repo.UpdateAsync(gameNight), Times.Once);
        }

        //----------Foutieve AddSnackToGameNightAsync tests------------------->
        [Fact]
        public async Task AddSnackToGameNightAsync_GameNightNotFound_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var snackName = "Chips";
            var isVegetarian = true;
            var isLactoseFree = true;
            var isNutFree = true;
            var isAlcoholFree = true;

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync((GameNight)null!); // Simuleer dat de GameNight niet gevonden wordt

            // Act
            var (success, errorMessage) = await _service.AddSnackToGameNightAsync(gameNightId, snackName, isVegetarian, isLactoseFree, isNutFree, isAlcoholFree);

            // Assert
            Assert.False(success);
            Assert.Equal("GameNight not found.", errorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Never);
        }

        //----------Test voor een succesvolle AddGameToGameNightAsync------------------->
        [Fact]
        public async Task AddGameToGameNightAsync_ValidGameAndGameNight_ReturnsSuccess()
        {
            // Arrange
            var gameNightId = 1;
            var gameId = 100;
            var game = new Game { Id = gameId, Name = "Catan", is18Plus = false };
            var gameNight = new GameNight
            {
                Id = gameNightId,
                GameNightGames = new List<GameNightGame>()
            };

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync(gameNight);
            _mockRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            var (success, message) = await _service.AddGameToGameNightAsync(gameNightId, gameId);

            // Assert
            Assert.True(success);
            Assert.Equal($"{game.Name} added to GameNight!", message);
            Assert.Single(gameNight.GameNightGames);
            Assert.Equal(gameId, gameNight.GameNightGames.First().GameId);
            _mockRepository.Verify(repo => repo.UpdateAsync(gameNight), Times.Once);
        }

        //----------Foutieve AddGameToGameNightAsync tests------------------->
        [Fact]
        public async Task AddGameToGameNightAsync_GameNightNotFound_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var gameId = 100;

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync((GameNight)null!); // Simuleer dat de GameNight niet wordt gevonden

            // Act
            var (success, message) = await _service.AddGameToGameNightAsync(gameNightId, gameId);

            // Assert
            Assert.False(success);
            Assert.Equal("GameNight not found", message);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Never);
        }

        [Fact]
        public async Task AddGameToGameNightAsync_GameNotFound_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var gameId = 100;
            var gameNight = new GameNight
            {
                Id = gameNightId,
                GameNightGames = new List<GameNightGame>()
            };

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync(gameNight);
            _mockRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null!); // Simuleer dat het spel niet wordt gevonden

            // Act
            var (success, message) = await _service.AddGameToGameNightAsync(gameNightId, gameId);

            // Assert
            Assert.False(success);
            Assert.Equal("Game not found", message);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Never);
        }

        [Fact]
        public async Task AddGameToGameNightAsync_GameAlreadyAdded_ReturnsError()
        {
            // Arrange
            var gameNightId = 1;
            var gameId = 100;
            var game = new Game { Id = gameId, Name = "Catan" };
            var gameNight = new GameNight
            {
                Id = gameNightId,
                GameNightGames = new List<GameNightGame>
        {
            new GameNightGame { GameNightId = gameNightId, GameId = gameId }
        }
            };

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId))
                .ReturnsAsync(gameNight);
            _mockRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            var (success, message) = await _service.AddGameToGameNightAsync(gameNightId, gameId);

            // Assert
            Assert.False(success);
            Assert.Equal("Game is already added to this GameNight.", message);
            _mockRepository.Verify(repo => repo.UpdateAsync(gameNight), Times.Never);
        }

        // Test voor een succesvolle GetAllGameNightsAsync()
        [Fact]
        public async Task GetAllGameNightsAsync_ReturnsGameNights_WithOrganizerDetails()
        {
            // Arrange
            var gameNights = new List<GameNight>
            {
                new GameNight { Id = 1, OrganizerId = "user123" },
                new GameNight { Id = 2, OrganizerId = "user456" }
            };

            var organizer1 = new Player { Id = "user123", UserName = "Organizer1" };
            var organizer2 = new Player { Id = "user456", UserName = "Organizer2" };

            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(gameNights);
            _mockUserManager.Setup(um => um.FindByIdAsync("user123")).ReturnsAsync(organizer1);
            _mockUserManager.Setup(um => um.FindByIdAsync("user456")).ReturnsAsync(organizer2);

            // Act
            var result = await _service.GetAllGameNightsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Organizer1", result?.First().Organizer?.UserName);
            Assert.Equal("Organizer2", result?.Last().Organizer?.UserName);
        }

        // Test voor het scenario waarbij er geen game nights zijn
        [Fact]
        public async Task GetAllGameNightsAsync_ReturnsEmptyList_WhenNoGameNightsExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<GameNight>());

            // Act
            var result = await _service.GetAllGameNightsAsync();

            // Assert
            Assert.Empty(result);
        }

        // Test voor het scenario waarbij de organizer niet wordt gevonden
        [Fact]
        public async Task GetAllGameNightsAsync_OrganizerNotFound_ReturnsGameNightWithNullOrganizer()
        {
            // Arrange
            var gameNights = new List<GameNight>
            {
                new GameNight { Id = 1, OrganizerId = "nonexistent" }
            };

            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(gameNights);
            _mockUserManager.Setup(um => um.FindByIdAsync("nonexistent")).ReturnsAsync((Player)null!); // Organizer wordt niet gevonden

            // Act
            var result = await _service.GetAllGameNightsAsync();

            // Assert
            Assert.Single(result);
            Assert.Null(result.First().Organizer); // Organizer is null
        }

        // Test voor een geldige GameNightId
        [Fact]
        public async Task GetGameNightByIdAsync_ValidId_ReturnsGameNight()
        {
            // Arrange
            var gameNightId = 1;
            var expectedGameNight = new GameNight { Id = gameNightId, Address = "123 Board Game St" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(gameNightId))
                           .ReturnsAsync(expectedGameNight);

            // Act
            var result = await _service.GetGameNightByIdAsync(gameNightId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGameNight.Id, result.Id);
            Assert.Equal(expectedGameNight.Address, result.Address);
            _mockRepository.Verify(repo => repo.GetByIdAsync(gameNightId), Times.Once);
        }

        // Test voor een niet-bestaande GameNightId
        [Fact]
        public async Task GetGameNightByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var gameNightId = 999; // Niet-bestaande ID
            _mockRepository.Setup(repo => repo.GetByIdAsync(gameNightId))
                           .ReturnsAsync((GameNight)null!);

            // Act
            var result = await _service.GetGameNightByIdAsync(gameNightId);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(gameNightId), Times.Once);
        }

        // Test voor het ophalen van GameNight details zonder een organizer
        [Fact]
        public async Task GetGameNightDetailsAsync_ValidIdWithoutOrganizer_ReturnsGameNightWithoutOrganizer()
        {
            // Arrange
            var gameNightId = 1;
            var expectedGameNight = new GameNight { Id = gameNightId, OrganizerId = null };

            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId)).ReturnsAsync(expectedGameNight);

            // Act
            var result = await _service.GetGameNightDetailsAsync(gameNightId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGameNight.Id, result.Id);
            Assert.Null(result.Organizer);
            _mockRepository.Verify(repo => repo.GetByIdWithDetailsAsync(gameNightId), Times.Once);
            _mockUserManager.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        // Test voor een niet-bestaande GameNight ID
        [Fact]
        public async Task GetGameNightDetailsAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var gameNightId = 999; // Niet-bestaande ID
            _mockRepository.Setup(repo => repo.GetByIdWithDetailsAsync(gameNightId)).ReturnsAsync((GameNight)null!);

            // Act
            var result = await _service.GetGameNightDetailsAsync(gameNightId);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByIdWithDetailsAsync(gameNightId), Times.Once);
            _mockUserManager.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        // Test voor het ophalen van GameNights waaraan de gebruiker heeft deelgenomen
        [Fact]
        public async Task GetParticipatedGameNightsAsync_ValidUserId_ReturnsParticipatedGameNights()
        {
            // Arrange
            var userId = "user123";
            var participatedGameNights = new List<GameNight>
            {
                new GameNight { Id = 1, Address = "Address 1" },
                new GameNight { Id = 2, Address = "Address 2" }
            };

            _mockRepository.Setup(repo => repo.GetParticipatedGamesByUserAsync(userId)).ReturnsAsync(participatedGameNights);

            // Act
            var result = await _service.GetParticipatedGameNightsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(participatedGameNights.Count, result.Count());
            Assert.Equal(participatedGameNights[0].Address, result?.First()?.Address);
            _mockRepository.Verify(repo => repo.GetParticipatedGamesByUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetParticipatedGameNightsAsync_EmptyUserId_ThrowsArgumentException()
        {
            // Arrange
            var emptyUserId = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetParticipatedGameNightsAsync(emptyUserId));
            _mockRepository.Verify(repo => repo.GetParticipatedGamesByUserAsync(It.IsAny<string>()), Times.Never);
        }

        // Test voor het controleren of een GameNight bestaat
        [Fact]
        public async Task GameNightExistsAsync_GameNightExists_ReturnsTrue()
        {
            // Arrange
            var gameNightId = 1;
            _mockRepository.Setup(repo => repo.GameNightExistsAsync(gameNightId)).ReturnsAsync(true);

            // Act
            var result = await _service.GameNightExistsAsync(gameNightId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.GameNightExistsAsync(gameNightId), Times.Once);
        }

        [Fact]
        public async Task GameNightExistsAsync_GameNightDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var gameNightId = 1;
            _mockRepository.Setup(repo => repo.GameNightExistsAsync(gameNightId)).ReturnsAsync(false);

            // Act
            var result = await _service.GameNightExistsAsync(gameNightId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(repo => repo.GameNightExistsAsync(gameNightId), Times.Once);
        }
    }
}
