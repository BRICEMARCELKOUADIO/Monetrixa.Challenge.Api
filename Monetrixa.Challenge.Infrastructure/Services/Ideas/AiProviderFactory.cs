using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Ideas;

public class AiProviderFactory : IAiProviderFactory
{
    private readonly IEnumerable<IIdeaGenerationAiService> _providers;

    public AiProviderFactory(IEnumerable<IIdeaGenerationAiService> providers)
    {
        _providers = providers;
    }

    public IIdeaGenerationAiService GetProvider(AiProvider provider)
    {
        var service = _providers.FirstOrDefault(x => x.Provider == provider);

        if (service is null)
        {
            throw new InvalidOperationException($"Aucun provider IA enregistré pour {provider}.");
        }

        return service;
    }
}