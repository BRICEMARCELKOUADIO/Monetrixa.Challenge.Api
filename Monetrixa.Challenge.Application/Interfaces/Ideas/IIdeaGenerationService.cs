using Monetrixa.ChallengeApp.Application.DTOs.Ideas;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Ideas;

public interface IIdeaGenerationService
{
    Task<IdeaGenerationResponse> GenerateIdeasAsync(
        GenerateIdeasRequest request,
        CancellationToken cancellationToken = default);

    Task<List<IdeaGenerationHistoryItemResponse>> GetHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<IdeaGenerationResponse> GetByIdAsync(
        Guid ideaGenerationId,
        CancellationToken cancellationToken = default);
}