using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Domain.Enums;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Ideas;

public interface IIdeaGenerationAiService
{
    AiProvider Provider { get; }

    Task<List<AiGeneratedIdeaResult>> GenerateIdeasAsync(
        string topic,
        MoodType mood,
        PlatformType platform,
        string challengeTheme,
        string? challengeQuote,
        CancellationToken cancellationToken = default);
}