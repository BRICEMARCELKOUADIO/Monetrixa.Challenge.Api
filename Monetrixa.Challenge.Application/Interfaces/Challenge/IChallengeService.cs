using Monetrixa.ChallengeApp.Application.DTOs.Challenge;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Challenge;

public interface IChallengeService
{
    Task<ChallengeSummaryResponse> JoinChallengeAsync(
        JoinChallengeRequest request,
        CancellationToken cancellationToken = default);

    Task<ChallengeSummaryResponse> GetCurrentChallengeAsync(
        CancellationToken cancellationToken = default);
}