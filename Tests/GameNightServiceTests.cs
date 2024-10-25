using System.Threading.Tasks;
using Xunit;
using Moq;
using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;

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
            Assert.Equal("New Address", updatedResult.Address); // Controleer of het adres is bijgewerkt
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GameNight>()), Times.Once);
        }

    }
}
