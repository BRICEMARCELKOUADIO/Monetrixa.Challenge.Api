using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;
using Monetrixa.ChallengeApp.Domain.Enums;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Ideas;

public class FakeIdeaGenerationAiService : IIdeaGenerationAiService
{
    public AiProvider Provider => AiProvider.OpenAI;

    public Task<List<AiGeneratedIdeaResult>> GenerateIdeasAsync(
        string topic,
        MoodType mood,
        PlatformType platform,
        string challengeTheme,
        string? challengeQuote,
        CancellationToken cancellationToken = default)
    {
        var ideas = new List<AiGeneratedIdeaResult>
        {
            new()
            {
                Title = $"Idée 1 - {topic}",
                Content = $"Crée un contenu {platform} simple sur '{topic}' en lien avec le thème '{challengeTheme}'. Humeur du jour : {mood}."
            },
            new()
            {
                Title = $"Idée 2 - {topic}",
                Content = $"Partage une astuce concrète sur '{topic}' adaptée à {platform}, avec un angle inspiré par '{challengeTheme}'."
            },
            new()
            {
                Title = $"Idée 3 - {topic}",
                Content = $"Raconte une expérience ou un retour terrain sur '{topic}' en gardant un ton compatible avec l’humeur '{mood}'."
            },
            new()
            {
                Title = $"Idée 4 - {topic}",
                Content = $"Propose un format court et actionnable sur '{topic}', lié au thème '{challengeTheme}'"
                          + (string.IsNullOrWhiteSpace(challengeQuote) ? "." : $" et inspiré par la citation '{challengeQuote}'.")
            }
        };

        return Task.FromResult(ideas);
    }
}