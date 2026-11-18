using System.Security.Claims;
using BordspelAvond.Application.Interfaces;
using BordspelAvond.Core.Domain;
using BordspelAvond.Presentation.Controllers;
using BordspelAvond.Presentation.ApiControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BordspelAvond.Tests;

public class GameNightControllerSecurityTests
{
    [Theory]
    [InlineData("organizer", true)]
    [InlineData("other-user", false)]
    public async Task ApiAddGame_ChecksStoredOwner(string userId, bool allowed)
    {
        var service = new Mock<IGameNightService>();
        service.Setup(s => s.GetGameNightByIdAsync(42))
            .ReturnsAsync(new GameNight { Id = 42, OrganizerId = "organizer" });
        service.Setup(s => s.AddGameToGameNightAsync(42, 7)).ReturnsAsync((true, "Game added"));
        var users = new Mock<UserManager<Player>>(Mock.Of<IUserStore<Player>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        users.Setup(u => u.FindByEmailAsync("player@example.com"))
            .ReturnsAsync(new Player { Id = userId });
        var controller = new GameNightApiController(service.Object, users.Object,
            NullLogger<GameNightApiController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "player@example.com")
                    }, "Bearer"))
                }
            }
        };

        var result = await controller.AddGameToGameNight(42, 7);

        if (allowed)
            Assert.IsType<OkObjectResult>(result);
        else
            Assert.Contains("Bearer", Assert.IsType<ForbidResult>(result).AuthenticationSchemes);
        service.Verify(s => s.AddGameToGameNightAsync(It.IsAny<int>(), It.IsAny<int>()),
            allowed ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public async Task Join_PreservesServiceArgumentsAndResult(bool withSnack, bool success)
    {
        var service = new Mock<IGameNightService>();
        service.Setup(s => s.JoinGameNightAsync(42, "player"))
            .ReturnsAsync((success, success ? null : "This GameNight is full."));
        service.Setup(s => s.JoinGameNightAsync(42, "player", "Fruit", true, true, false, true))
            .ReturnsAsync((success, success ? null : "This GameNight is full."));
        using var context = new BordspelDbContext(new DbContextOptions<BordspelDbContext>());
        var controller = CreateController(context, service, "player");
        controller.Url = Mock.Of<IUrlHelper>();

        var result = withSnack
            ? await controller.JoinWithSnack(42, "Fruit", true, true, false, true)
            : await controller.Join(42);

        if (withSnack)
            service.Verify(s => s.JoinGameNightAsync(42, "player", "Fruit", true, true, false, true), Times.Once);
        else
            service.Verify(s => s.JoinGameNightAsync(42, "player"), Times.Once);
        if (!success)
            Assert.Equal("This GameNight is full.", controller.TempData["JoinError"]);
        if (withSnack && !success)
            Assert.StartsWith("/Identity/Account/Login", Assert.IsType<RedirectResult>(result).Url);
        else
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal(42, redirect.RouteValues!["id"]);
        }
    }

    [Theory]
    [InlineData("organizer", "organizer", true)]
    [InlineData("other-user", "organizer", false)]
    [InlineData("other-user", null, false)]
    public async Task AddGame_ChecksStoredOwnerBeforeMutation(string userId, string? organizerId, bool allowed)
    {
        var service = new Mock<IGameNightService>();
        service.Setup(s => s.GetGameNightByIdAsync(42))
            .ReturnsAsync(new GameNight { Id = 42, OrganizerId = organizerId });
        service.Setup(s => s.AddGameToGameNightAsync(42, 7)).ReturnsAsync((true, "Game added"));
        using var context = new BordspelDbContext(new DbContextOptions<BordspelDbContext>());
        var controller = CreateController(context, service, userId);

        var result = await controller.AddGameToGameNight(42, 7);

        if (allowed)
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal(42, redirect.RouteValues!["id"]);
            Assert.Equal("Game added", controller.TempData["GameAddSuccess"]);
        }
        else
        {
            Assert.IsType<ForbidResult>(result);
        }
        service.Verify(s => s.AddGameToGameNightAsync(It.IsAny<int>(), It.IsAny<int>()),
            allowed ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task AddGame_WithoutUser_DoesNotReadOrModifyGameNight()
    {
        var service = new Mock<IGameNightService>(MockBehavior.Strict);
        using var context = new BordspelDbContext(new DbContextOptions<BordspelDbContext>());
        var controller = CreateController(context, service, null);
        Assert.IsType<ChallengeResult>(await controller.AddGameToGameNight(42, 7));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddGame_MissingGameNight_PreservesErrorRedirect()
    {
        var service = new Mock<IGameNightService>();
        using var context = new BordspelDbContext(new DbContextOptions<BordspelDbContext>());
        var controller = CreateController(context, service, "organizer");
        var result = Assert.IsType<RedirectToActionResult>(await controller.AddGameToGameNight(42, 7));
        Assert.Equal("Details", result.ActionName);
        Assert.Equal(42, result.RouteValues!["id"]);
        Assert.Equal("GameNight not found", controller.TempData["GameAddError"]);
        service.Verify(s => s.AddGameToGameNightAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(nameof(GameNightController.Join))]
    [InlineData(nameof(GameNightController.JoinWithSnack))]
    [InlineData(nameof(GameNightController.AddGameToGameNight))]
    public void Mutations_RequirePostAndAntiforgery(string action)
    {
        var method = typeof(GameNightController).GetMethod(action)!;
        Assert.NotEmpty(method.GetCustomAttributes(typeof(HttpPostAttribute), true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
    }

    [Fact]
    public void AddGame_RequiresAuthentication()
    {
        var method = typeof(GameNightController).GetMethod(nameof(GameNightController.AddGameToGameNight))!;
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }

    private static GameNightController CreateController(BordspelDbContext context,
        Mock<IGameNightService> service, string? userId)
    {
        var users = new Mock<UserManager<Player>>(Mock.Of<IUserStore<Player>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        users.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        var httpContext = new DefaultHttpContext();
        return new GameNightController(context, service.Object, users.Object,
            NullLogger<GameNightController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }
}
