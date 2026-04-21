using FluentAssertions;
using Monetrixa.ChallengeApp.Application.DTOs.Auth;
using Monetrixa.ChallengeApp.Application.Interfaces.Auth;
using Monetrixa.ChallengeApp.Infrastructure.Services.Auth;
using Monetrixa.ChallengeApp.Tests.Common;
using Moq;

namespace Monetrixa.ChallengeApp.Tests.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_Should_Create_User_And_Return_AuthResponse()
    {
        // Arrange
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var passwordService = new PasswordService();

            var jwtTokenServiceMock = new Mock<IJwtTokenService>();
            jwtTokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<Monetrixa.ChallengeApp.Domain.Entities.User>()))
                .Returns("fake-jwt-token");

            var expiration = DateTime.UtcNow.AddHours(2);
            jwtTokenServiceMock
                .Setup(x => x.GetExpirationUtc())
                .Returns(expiration);

            var authService = new AuthService(
                dbContext,
                passwordService,
                jwtTokenServiceMock.Object);

            var request = new RegisterRequest
            {
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                Password = "Brice@12345"
            };

            // Act
            var result = await authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("fake-jwt-token");
            result.Email.Should().Be("brice@test.com");
            result.FullName.Should().Be("Brice Marcel");
            result.Role.Should().Be("Participant");
            result.ExpiresAtUtc.Should().Be(expiration);

            dbContext.Users.Count().Should().Be(1);

            var createdUser = dbContext.Users.Single();
            createdUser.Email.Should().Be("brice@test.com");
            createdUser.FullName.Should().Be("Brice Marcel");
            createdUser.PasswordHash.Should().NotBeNullOrWhiteSpace();
            createdUser.PasswordHash.Should().NotBe("Brice@12345");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_When_Email_Already_Exists()
    {
        // Arrange
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var passwordService = new PasswordService();

            var jwtTokenServiceMock = new Mock<IJwtTokenService>();
            jwtTokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<Monetrixa.ChallengeApp.Domain.Entities.User>()))
                .Returns("fake-jwt-token");

            jwtTokenServiceMock
                .Setup(x => x.GetExpirationUtc())
                .Returns(DateTime.UtcNow.AddHours(2));

            var authService = new AuthService(
                dbContext,
                passwordService,
                jwtTokenServiceMock.Object);

            var request = new RegisterRequest
            {
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                Password = "Brice@12345"
            };

            await authService.RegisterAsync(request);

            // Act
            var action = async () => await authService.RegisterAsync(request);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Un compte existe déjà avec cet email.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task LoginAsync_Should_Return_AuthResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var passwordService = new PasswordService();

            var jwtTokenServiceMock = new Mock<IJwtTokenService>();
            jwtTokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<Monetrixa.ChallengeApp.Domain.Entities.User>()))
                .Returns("fake-jwt-token");

            var expiration = DateTime.UtcNow.AddHours(2);
            jwtTokenServiceMock
                .Setup(x => x.GetExpirationUtc())
                .Returns(expiration);

            var authService = new AuthService(
                dbContext,
                passwordService,
                jwtTokenServiceMock.Object);

            var registerRequest = new RegisterRequest
            {
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                Password = "Brice@12345"
            };

            await authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "brice@test.com",
                Password = "Brice@12345"
            };

            // Act
            var result = await authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("fake-jwt-token");
            result.Email.Should().Be("brice@test.com");
            result.FullName.Should().Be("Brice Marcel");
            result.Role.Should().Be("Participant");
            result.ExpiresAtUtc.Should().Be(expiration);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task LoginAsync_Should_Throw_When_Email_Does_Not_Exist()
    {
        // Arrange
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var passwordService = new PasswordService();

            var jwtTokenServiceMock = new Mock<IJwtTokenService>();
            jwtTokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<Monetrixa.ChallengeApp.Domain.Entities.User>()))
                .Returns("fake-jwt-token");

            jwtTokenServiceMock
                .Setup(x => x.GetExpirationUtc())
                .Returns(DateTime.UtcNow.AddHours(2));

            var authService = new AuthService(
                dbContext,
                passwordService,
                jwtTokenServiceMock.Object);

            var loginRequest = new LoginRequest
            {
                Email = "unknown@test.com",
                Password = "Brice@12345"
            };

            // Act
            var action = async () => await authService.LoginAsync(loginRequest);

            // Assert
            await action.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Email ou mot de passe invalide.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task LoginAsync_Should_Throw_When_Password_Is_Invalid()
    {
        // Arrange
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var passwordService = new PasswordService();

            var jwtTokenServiceMock = new Mock<IJwtTokenService>();
            jwtTokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<Monetrixa.ChallengeApp.Domain.Entities.User>()))
                .Returns("fake-jwt-token");

            jwtTokenServiceMock
                .Setup(x => x.GetExpirationUtc())
                .Returns(DateTime.UtcNow.AddHours(2));

            var authService = new AuthService(
                dbContext,
                passwordService,
                jwtTokenServiceMock.Object);

            var registerRequest = new RegisterRequest
            {
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                Password = "Brice@12345"
            };

            await authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "brice@test.com",
                Password = "WrongPassword"
            };

            // Act
            var action = async () => await authService.LoginAsync(loginRequest);

            // Assert
            await action.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Email ou mot de passe invalide.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }
}