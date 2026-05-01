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

    [Fact]
    public async Task GetMyChallengeDaysAsync_Should_Return_Ordered_Days_For_Current_Challenge()
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

            var day1Id = Guid.NewGuid();
            var day2Id = Guid.NewGuid();

            dbContext.ChallengeDays.AddRange(
                new ChallengeDay
                {
                    Id = day2Id,
                    ChallengeId = challengeId,
                    DayNumber = 2,
                    WeekNumber = 1,
                    Theme = "Jour 2",
                    Quote = "Citation 2"
                },
                new ChallengeDay
                {
                    Id = day1Id,
                    ChallengeId = challengeId,
                    DayNumber = 1,
                    WeekNumber = 1,
                    Theme = "Jour 1",
                    Quote = "Citation 1"
                });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            dbContext.DailyValidations.Add(new DailyValidation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeDayId = day1Id,
                Status = ValidationStatus.Validated,
                SubmittedAtUtc = DateTime.UtcNow,
                ValidatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetMyChallengeDaysAsync();

            result.Should().NotBeNull();
            result.ChallengeId.Should().Be(challengeId);
            result.Title.Should().Be("Challenge 30 jours");
            result.Days.Should().HaveCount(2);
            result.Days[0].DayNumber.Should().Be(1);
            result.Days[1].DayNumber.Should().Be(2);
            result.Days[0].IsValidated.Should().BeTrue();
            result.Days[1].IsValidated.Should().BeFalse();
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetMyChallengeDaysAsync_Should_Throw_When_User_Has_No_Challenge()
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

            var action = async () => await service.GetMyChallengeDaysAsync();

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Aucun challenge rejoint pour cet utilisateur.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task ValidateChallengeDayAsync_Should_Create_Validation_When_Request_Is_Valid()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();

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

            dbContext.ChallengeDays.Add(new ChallengeDay
            {
                Id = challengeDayId,
                ChallengeId = challengeId,
                DayNumber = 1,
                WeekNumber = 1,
                Theme = "Jour 1",
                Quote = "Citation 1"
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

            var request = new ValidateChallengeDayRequest
            {
                ChallengeDayId = challengeDayId,
                Note = "Journée réalisée"
            };

            var result = await service.ValidateChallengeDayAsync(request);

            result.Should().NotBeNull();
            result.ChallengeDayId.Should().Be(challengeDayId);
            result.Status.Should().Be("Validated");
            result.Note.Should().Be("Journée réalisée");

            dbContext.DailyValidations.Count().Should().Be(1);
            dbContext.DailyValidations.Single().UserId.Should().Be(userId);
            dbContext.DailyValidations.Single().ChallengeDayId.Should().Be(challengeDayId);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task ValidateChallengeDayAsync_Should_Throw_When_User_Has_No_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();

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

            var request = new ValidateChallengeDayRequest
            {
                ChallengeDayId = challengeDayId,
                Note = "Test"
            };

            var action = async () => await service.ValidateChallengeDayAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Aucun challenge rejoint pour cet utilisateur.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task ValidateChallengeDayAsync_Should_Throw_When_Day_Does_Not_Belong_To_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var otherChallengeId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();

            dbContext.Users.Add(new User
            {
                Id = userId,
                FullName = "Brice Marcel",
                Email = "brice@test.com",
                PasswordHash = "hash",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Challenges.AddRange(
                new Challenge
                {
                    Id = challengeId,
                    Title = "Challenge 30 jours",
                    AccessCode = "START2026",
                    Description = "Test",
                    StartDateUtc = DateTime.UtcNow.Date,
                    EndDateUtc = DateTime.UtcNow.Date.AddDays(29),
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = otherChallengeId,
                    Title = "Autre challenge",
                    AccessCode = "OTHER2026",
                    Description = "Test",
                    StartDateUtc = DateTime.UtcNow.Date,
                    EndDateUtc = DateTime.UtcNow.Date.AddDays(29),
                    CreatedAtUtc = DateTime.UtcNow
                });

            dbContext.ChallengeDays.Add(new ChallengeDay
            {
                Id = challengeDayId,
                ChallengeId = otherChallengeId,
                DayNumber = 1,
                WeekNumber = 1,
                Theme = "Jour 1",
                Quote = "Citation 1"
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

            var request = new ValidateChallengeDayRequest
            {
                ChallengeDayId = challengeDayId,
                Note = "Test"
            };

            var action = async () => await service.ValidateChallengeDayAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Jour de challenge introuvable pour cet utilisateur.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task ValidateChallengeDayAsync_Should_Throw_When_Day_Is_Already_Validated()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();

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

            dbContext.ChallengeDays.Add(new ChallengeDay
            {
                Id = challengeDayId,
                ChallengeId = challengeId,
                DayNumber = 1,
                WeekNumber = 1,
                Theme = "Jour 1",
                Quote = "Citation 1"
            });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            dbContext.DailyValidations.Add(new DailyValidation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeDayId = challengeDayId,
                Status = ValidationStatus.Validated,
                Note = "Déjà validé",
                SubmittedAtUtc = DateTime.UtcNow,
                ValidatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new ValidateChallengeDayRequest
            {
                ChallengeDayId = challengeDayId,
                Note = "Nouvelle validation"
            };

            var action = async () => await service.ValidateChallengeDayAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cette journée a déjà été validée.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }
}