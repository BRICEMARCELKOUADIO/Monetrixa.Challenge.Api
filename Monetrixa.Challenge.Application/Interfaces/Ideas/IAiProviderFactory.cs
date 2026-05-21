using Monetrixa.ChallengeApp.Application.Common.Options;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Ideas;

public interface IAiProviderFactory
{
    IIdeaGenerationAiService GetProvider(AiProvider provider);
}