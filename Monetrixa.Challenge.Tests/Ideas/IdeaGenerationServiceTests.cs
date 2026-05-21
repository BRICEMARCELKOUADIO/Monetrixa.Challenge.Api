using Microsoft.Extensions.Options;
using Moq;
using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Application.Interfaces.Common;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Domain.Enums;
using Monetrixa.ChallengeApp.Infrastructure.Services.Ideas;
using Monetrixa.ChallengeApp.Tests.Common;

namespace Monetrixa.ChallengeApp.Tests.Ideas;

public class IdeaGenerationServiceTests
{
    [Fact]
    public async Task GenerateIdeasAsync_Should_Create_Generation_And_GeneratedIdeas()
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

            var aiServiceMock = new Mock<IIdeaGenerationAiService>();
            aiServiceMock.Setup(x => x.Provider).Returns(AiProvider.OpenAI);
            aiServiceMock
                .Setup(x => x.GenerateIdeasAsync(
                    It.IsAny<string>(),
                    It.IsAny<MoodType>(),
                    It.IsAny<PlatformType>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AiGeneratedIdeaResult>
                {
                    new() { Title = "Idée 1", Content = "Contenu 1" },
                    new() { Title = "Idée 2", Content = "Contenu 2" },
                    new() { Title = "Idée 3", Content = "Contenu 3" },
                    new() { Title = "Idée 4", Content = "Contenu 4" }
                });

            var aiProviderFactoryMock = new Mock<IAiProviderFactory>();
            aiProviderFactoryMock
                .Setup(x => x.GetProvider(AiProvider.OpenAI))
                .Returns(aiServiceMock.Object);

            var aiOptions = Options.Create(new AiOptions
            {
                Provider = "OpenAI",
                ApiKey = "test-key",
                Model = "gpt-4o-mini",
                BaseUrl = "https://api.openai.com/v1/"
            });

            var service = new IdeaGenerationService(
                dbContext,
                currentUserServiceMock.Object,
                aiProviderFactoryMock.Object,
                aiOptions);

            var request = new GenerateIdeasRequest
            {
                ChallengeDayId = challengeDayId,
                Topic = "Finance personnelle",
                Mood = MoodType.Motivated,
                Platform = PlatformType.Instagram
            };

            var result = await service.GenerateIdeasAsync(request);

            Assert.Equal("Finance personnelle", result.Topic);
            Assert.Equal(4, result.Ideas.Count);
            Assert.Single(dbContext.IdeaGenerations);
            Assert.Equal(4, dbContext.GeneratedIdeas.Count());
        }
        finally
        {
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();
        }
    }
}