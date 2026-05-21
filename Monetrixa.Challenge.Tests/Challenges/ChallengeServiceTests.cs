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

    [Fact]
    public async Task CreatePublishedContentAsync_Should_Create_Content_When_Request_Is_Valid()
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

            var request = new CreatePublishedContentRequest
            {
                ChallengeDayId = challengeDayId,
                Title = "Mon post du jour",
                Description = "Description test",
                Url = "https://tiktok.com/test-video",
                Platform = PlatformType.TikTok,
                ThumbnailUrl = "https://img.test/thumb.jpg",
                PublishedAtUtc = DateTime.UtcNow
            };

            var result = await service.CreatePublishedContentAsync(request);

            result.Should().NotBeNull();
            result.ChallengeDayId.Should().Be(challengeDayId);
            result.Title.Should().Be("Mon post du jour");
            result.Platform.Should().Be("TikTok");

            dbContext.PublishedContents.Count().Should().Be(1);
            dbContext.PublishedContents.Single().UserId.Should().Be(userId);
            dbContext.PublishedContents.Single().ChallengeDayId.Should().Be(challengeDayId);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreatePublishedContentAsync_Should_Throw_When_User_Has_No_Challenge()
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

            var request = new CreatePublishedContentRequest
            {
                ChallengeDayId = challengeDayId,
                Title = "Titre",
                Url = "https://youtube.com/test",
                Platform = PlatformType.YouTube,
                PublishedAtUtc = DateTime.UtcNow
            };

            var action = async () => await service.CreatePublishedContentAsync(request);

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
    public async Task CreatePublishedContentAsync_Should_Throw_When_Day_Does_Not_Belong_To_Current_Challenge()
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

            var request = new CreatePublishedContentRequest
            {
                ChallengeDayId = challengeDayId,
                Title = "Titre",
                Url = "https://youtube.com/test",
                Platform = PlatformType.YouTube,
                PublishedAtUtc = DateTime.UtcNow
            };

            var action = async () => await service.CreatePublishedContentAsync(request);

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
    public async Task CreateCommentAsync_Should_Create_Comment_When_Request_Is_Valid()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();
            var publishedContentId = Guid.NewGuid();

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

            dbContext.PublishedContents.Add(new PublishedContent
            {
                Id = publishedContentId,
                UserId = userId,
                ChallengeDayId = challengeDayId,
                Title = "Contenu",
                Url = "https://test.com/content",
                Platform = PlatformType.Instagram,
                PublishedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new CreateCommentRequest
            {
                PublishedContentId = publishedContentId,
                Content = "Très bon contenu"
            };

            var result = await service.CreateCommentAsync(request);

            result.Content.Should().Be("Très bon contenu");
            result.UserFullName.Should().Be("Brice Marcel");
            dbContext.Comments.Count().Should().Be(1);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateCommentAsync_Should_Throw_When_PublishedContent_Does_Not_Exist()
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

            var request = new CreateCommentRequest
            {
                PublishedContentId = Guid.NewGuid(),
                Content = "Test"
            };

            var action = async () => await service.CreateCommentAsync(request);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Contenu publié introuvable.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCommentsByPublishedContentIdAsync_Should_Return_Comments_Ordered_By_Date()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var challengeDayId = Guid.NewGuid();
            var publishedContentId = Guid.NewGuid();

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

            dbContext.PublishedContents.Add(new PublishedContent
            {
                Id = publishedContentId,
                UserId = userId,
                ChallengeDayId = challengeDayId,
                Title = "Contenu",
                Url = "https://test.com/content",
                Platform = PlatformType.Instagram,
                PublishedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Comments.AddRange(
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PublishedContentId = publishedContentId,
                    UserId = userId,
                    Content = "Commentaire 1",
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PublishedContentId = publishedContentId,
                    UserId = userId,
                    Content = "Commentaire 2",
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1)
                });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetCommentsByPublishedContentIdAsync(publishedContentId);

            result.Should().HaveCount(2);
            result[0].Content.Should().Be("Commentaire 1");
            result[1].Content.Should().Be("Commentaire 2");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCurrentChallengeContentsAsync_Should_Return_Contents_For_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var otherChallengeId = Guid.NewGuid();
            var day1Id = Guid.NewGuid();
            var day2Id = Guid.NewGuid();
            var otherDayId = Guid.NewGuid();

            dbContext.Users.AddRange(
                new User
                {
                    Id = userId,
                    FullName = "Brice Marcel",
                    Email = "brice@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Participant,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new User
                {
                    Id = otherUserId,
                    FullName = "Autre User",
                    Email = "other@test.com",
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

            dbContext.ChallengeDays.AddRange(
                new ChallengeDay
                {
                    Id = day1Id,
                    ChallengeId = challengeId,
                    DayNumber = 1,
                    WeekNumber = 1,
                    Theme = "Jour 1",
                    Quote = "Citation 1"
                },
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
                    Id = otherDayId,
                    ChallengeId = otherChallengeId,
                    DayNumber = 1,
                    WeekNumber = 1,
                    Theme = "Autre jour",
                    Quote = "Autre citation"
                });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            var content1Id = Guid.NewGuid();
            var content2Id = Guid.NewGuid();

            dbContext.PublishedContents.AddRange(
                new PublishedContent
                {
                    Id = content1Id,
                    UserId = userId,
                    ChallengeDayId = day1Id,
                    Title = "Contenu 1",
                    Url = "https://test.com/1",
                    Platform = PlatformType.Instagram,
                    PublishedAtUtc = DateTime.UtcNow.AddMinutes(-10),
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
                },
                new PublishedContent
                {
                    Id = content2Id,
                    UserId = otherUserId,
                    ChallengeDayId = day2Id,
                    Title = "Contenu 2",
                    Url = "https://test.com/2",
                    Platform = PlatformType.TikTok,
                    PublishedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
                },
                new PublishedContent
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChallengeDayId = otherDayId,
                    Title = "Contenu hors challenge",
                    Url = "https://test.com/3",
                    Platform = PlatformType.YouTube,
                    PublishedAtUtc = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                });

            dbContext.Comments.AddRange(
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PublishedContentId = content1Id,
                    UserId = otherUserId,
                    Content = "Bravo",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PublishedContentId = content1Id,
                    UserId = userId,
                    Content = "Merci",
                    CreatedAtUtc = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetCurrentChallengeContentsAsync();

            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Contenu 2");
            result[1].Title.Should().Be("Contenu 1");
            result[1].CommentCount.Should().Be(2);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCurrentChallengeContentsAsync_Should_Throw_When_User_Has_No_Challenge()
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

            var action = async () => await service.GetCurrentChallengeContentsAsync();

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
    public async Task GetMyPublishedContentsAsync_Should_Return_Only_My_Contents_For_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var otherChallengeId = Guid.NewGuid();
            var day1Id = Guid.NewGuid();
            var day2Id = Guid.NewGuid();
            var otherDayId = Guid.NewGuid();

            dbContext.Users.AddRange(
                new User
                {
                    Id = userId,
                    FullName = "Brice Marcel",
                    Email = "brice@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Participant,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new User
                {
                    Id = otherUserId,
                    FullName = "Autre User",
                    Email = "other@test.com",
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

            dbContext.ChallengeDays.AddRange(
                new ChallengeDay
                {
                    Id = day1Id,
                    ChallengeId = challengeId,
                    DayNumber = 1,
                    WeekNumber = 1,
                    Theme = "Jour 1",
                    Quote = "Citation 1"
                },
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
                    Id = otherDayId,
                    ChallengeId = otherChallengeId,
                    DayNumber = 1,
                    WeekNumber = 1,
                    Theme = "Autre jour",
                    Quote = "Autre citation"
                });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            dbContext.PublishedContents.AddRange(
                new PublishedContent
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChallengeDayId = day1Id,
                    Title = "Mon contenu 1",
                    Url = "https://test.com/1",
                    Platform = PlatformType.Instagram,
                    PublishedAtUtc = DateTime.UtcNow.AddMinutes(-10),
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
                },
                new PublishedContent
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChallengeDayId = day2Id,
                    Title = "Mon contenu 2",
                    Url = "https://test.com/2",
                    Platform = PlatformType.TikTok,
                    PublishedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
                },
                new PublishedContent
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChallengeDayId = day1Id,
                    Title = "Contenu autre user",
                    Url = "https://test.com/3",
                    Platform = PlatformType.YouTube,
                    PublishedAtUtc = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new PublishedContent
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChallengeDayId = otherDayId,
                    Title = "Contenu autre challenge",
                    Url = "https://test.com/4",
                    Platform = PlatformType.Facebook,
                    PublishedAtUtc = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetMyPublishedContentsAsync();

            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Mon contenu 2");
            result[1].Title.Should().Be("Mon contenu 1");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetPublishedContentByIdAsync_Should_Return_Content_When_It_Belongs_To_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var dayId = Guid.NewGuid();
            var publishedContentId = Guid.NewGuid();

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
                Id = dayId,
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

            dbContext.PublishedContents.Add(new PublishedContent
            {
                Id = publishedContentId,
                UserId = userId,
                ChallengeDayId = dayId,
                Title = "Mon contenu",
                Description = "Description",
                Url = "https://test.com/content",
                Platform = PlatformType.Instagram,
                ThumbnailUrl = "https://test.com/thumb.jpg",
                PublishedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                PublishedContentId = publishedContentId,
                UserId = userId,
                Content = "Bravo",
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetPublishedContentByIdAsync(publishedContentId);

            result.Should().NotBeNull();
            result.PublishedContentId.Should().Be(publishedContentId);
            result.Title.Should().Be("Mon contenu");
            result.CommentCount.Should().Be(1);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetPublishedContentByIdAsync_Should_Throw_When_Content_Does_Not_Belong_To_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var otherChallengeId = Guid.NewGuid();
            var otherDayId = Guid.NewGuid();
            var publishedContentId = Guid.NewGuid();

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
                Id = otherDayId,
                ChallengeId = otherChallengeId,
                DayNumber = 1,
                WeekNumber = 1,
                Theme = "Autre jour",
                Quote = "Autre citation"
            });

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            dbContext.PublishedContents.Add(new PublishedContent
            {
                Id = publishedContentId,
                UserId = userId,
                ChallengeDayId = otherDayId,
                Title = "Contenu hors challenge",
                Url = "https://test.com/outside",
                Platform = PlatformType.YouTube,
                PublishedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var action = async () => await service.GetPublishedContentByIdAsync(publishedContentId);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Contenu publié introuvable pour cet utilisateur.");
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCurrentChallengeResourcesAsync_Should_Return_Only_Active_Resources_For_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var otherChallengeId = Guid.NewGuid();

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

            dbContext.UserChallenges.Add(new UserChallenge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChallengeId = challengeId,
                JoinedAtUtc = DateTime.UtcNow,
                Score = 0
            });

            dbContext.Resources.AddRange(
                new Resource
                {
                    Id = Guid.NewGuid(),
                    ChallengeId = challengeId,
                    Title = "PDF Guide",
                    Description = "Guide PDF",
                    ResourceType = ResourceType.Pdf,
                    Url = "https://test.com/guide.pdf",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Resource
                {
                    Id = Guid.NewGuid(),
                    ChallengeId = challengeId,
                    Title = "Vidéo intro",
                    Description = "Vidéo",
                    ResourceType = ResourceType.Video,
                    Url = "https://test.com/video.mp4",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Resource
                {
                    Id = Guid.NewGuid(),
                    ChallengeId = challengeId,
                    Title = "Inactive",
                    Description = "Inactive",
                    ResourceType = ResourceType.Link,
                    Url = "https://test.com/inactive",
                    DisplayOrder = 3,
                    IsActive = false,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Resource
                {
                    Id = Guid.NewGuid(),
                    ChallengeId = otherChallengeId,
                    Title = "Autre challenge",
                    Description = "Autre",
                    ResourceType = ResourceType.Audio,
                    Url = "https://test.com/audio.mp3",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetCurrentChallengeResourcesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Vidéo intro", result[0].Title);
            Assert.Equal("PDF Guide", result[1].Title);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetResourceByIdAsync_Should_Return_Resource_When_It_Belongs_To_Current_Challenge()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var userId = Guid.NewGuid();
            var challengeId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

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

            dbContext.Resources.Add(new Resource
            {
                Id = resourceId,
                ChallengeId = challengeId,
                Title = "PDF Guide",
                Description = "Guide PDF",
                ResourceType = ResourceType.Pdf,
                Url = "https://test.com/guide.pdf",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var result = await service.GetResourceByIdAsync(resourceId);

            Assert.Equal(resourceId, result.ResourceId);
            Assert.Equal("PDF Guide", result.Title);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateResourceAsync_Should_Create_Resource_When_Request_Is_Valid()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var challengeId = Guid.NewGuid();

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
            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new CreateResourceRequest
            {
                ChallengeId = challengeId,
                Title = "Guide PDF",
                Description = "Guide complet",
                ResourceType = ResourceType.Pdf,
                Url = "https://test.com/guide.pdf",
                FileName = "guide.pdf",
                ContentType = "application/pdf",
                ThumbnailUrl = "https://test.com/thumb.jpg",
                DisplayOrder = 1,
                IsActive = true
            };

            var result = await service.CreateResourceAsync(request);

            Assert.Equal("Guide PDF", result.Title);
            Assert.Equal(challengeId, result.ChallengeId);
            Assert.Single(dbContext.Resources);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateResourceAsync_Should_Update_Resource_When_Request_Is_Valid()
    {
        var (dbContext, connection) = SqliteTestDbContextFactory.Create();

        try
        {
            var challengeId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

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

            dbContext.Resources.Add(new Resource
            {
                Id = resourceId,
                ChallengeId = challengeId,
                Title = "Ancien titre",
                Description = "Ancienne description",
                ResourceType = ResourceType.Link,
                Url = "https://test.com/old",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var service = new ChallengeService(dbContext, currentUserServiceMock.Object);

            var request = new UpdateResourceRequest
            {
                Title = "Nouveau titre",
                Description = "Nouvelle description",
                ResourceType = ResourceType.Video,
                Url = "https://test.com/new",
                FileName = "video.mp4",
                ContentType = "video/mp4",
                ThumbnailUrl = "https://test.com/thumb.jpg",
                DisplayOrder = 2,
                IsActive = false
            };

            var result = await service.UpdateResourceAsync(resourceId, request);

            Assert.Equal("Nouveau titre", result.Title);
            Assert.Equal("Video", result.ResourceType);

            var resource = dbContext.Resources.Single();
            Assert.Equal("Nouveau titre", resource.Title);
            Assert.Equal(2, resource.DisplayOrder);
            Assert.False(resource.IsActive);
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }
}