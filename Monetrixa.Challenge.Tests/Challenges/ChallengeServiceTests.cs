using FluentAssertions;
using Monetrixa.ChallengeApp.Application.DTOs.Challenge;
using Monetrixa.ChallengeApp.Application.Interfaces.Common;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Domain.Enums;
using Monetrixa.ChallengeApp.Infrastructure.Services.Challenge;
using Monetrixa.ChallengeApp.Tests.Common;
using Moq;

namespace Monetrixa.ChallengeApp.Tests.Challenges;

public class ChallengeServiceTests
{
    [Fact]
    public async Task JoinChallengeAsync_Should_Create_UserChallenge_When_Code_Is_Valid()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                PasswordHash = "hash",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Challenges.Add(new Challenge
            {
                Id = challengeId,
                Title = "Challenge 30 jours",
                AccessCode = "START2026",
                Description = "Test",
                StartDateUtc = DateTime.UtcNow.Date,
                EndDateUtc = DateTime.UtcNow.Date.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new JoinChallengeRequest
            {
                AccessCode = "START2026"
            };

            var result = await service.JoinChallengeAsync(request);

            result.Should().NotBeNull();
            result.Title.Should().Be("Challenge 30 jours");

            dbContext.UserChallenges.Count().Should().Be(1);
            dbContext.UserChallenges.Single().UserId.Should().Be(userId);
            dbContext.UserChallenges.Single().ChallengeId.Should().Be(challengeId);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task JoinChallengeAsync_Should_Throw_When_Challenge_Does_Not_Exist()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new JoinChallengeRequest
            {
                AccessCode = "INVALID"
            };

            var action = async () => await service.JoinChallengeAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Challenge introuvable avec ce code.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task JoinChallengeAsync_Should_Throw_When_User_Already_Joined()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                PasswordHash = "hash",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Challenges.Add(new Challenge
            {
                Id = challengeId,
                Title = "Challenge 30 jours",
                AccessCode = "START2026",
                Description = "Test",
                StartDateUtc = DateTime.UtcNow.Date,
                EndDateUtc = DateTime.UtcNow.Date.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new JoinChallengeRequest
            {
                AccessCode = "START2026"
            };

            var action = async () => await service.JoinChallengeAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Vous avez déjà rejoint ce challenge.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCurrentChallengeAsync_Should_Return_Current_Challenge_When_User_Has_Joined()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                PasswordHash = "hash",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Challenges.Add(new Challenge
            {
                Id = challengeId,
                Title = "Challenge 30 jours",
                AccessCode = "START2026",
                Description = "Test",
                StartDateUtc = DateTime.UtcNow.Date,
                EndDateUtc = DateTime.UtcNow.Date.AddDays(29),
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetCurrentChallengeAsync();

            result.Should().NotBeNull();
            result.ChallengeId.Should().Be(challengeId);
            result.Title.Should().Be("Challenge 30 jours");
            result.AccessCode.Should().Be("START2026");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCurrentChallengeAsync_Should_Throw_When_User_Has_No_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                PasswordHash = "hash",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var action = async () => await service.GetCurrentChallengeAsync();

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Aucun challenge rejoint pour cet utilisateur.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }
}