using Monetrixa.ChallengeApp.Application.DTOs.Challenge;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Challenge;

public interface IChallengeService
{
    Task<ChallengeSummaryResponse> JoinChallengeAsync(
        JoinChallengeRequest request,
        CancellationToken cancellationToken = default);

    Task<ChallengeSummaryResponse> GetCurrentChallengeAsync(
        CancellationToken cancellationToken = default);

    Task<CurrentChallengeDaysResponse> GetMyChallengeDaysAsync(
    CancellationToken cancellationToken = default);

    Task<DailyValidationResponse> ValidateChallengeDayAsync(
    ValidateChallengeDayRequest request,
    CancellationToken cancellationToken = default);

    Task<PublishedContentResponse> CreatePublishedContentAsync(
    CreatePublishedContentRequest request,
    CancellationToken cancellationToken = default);

    Task<CommentResponse> CreateCommentAsync(
    CreateCommentRequest request,
    CancellationToken cancellationToken = default);

    Task<List<CommentResponse>> GetCommentsByPublishedContentIdAsync(
        Guid publishedContentId,
        CancellationToken cancellationToken = default);

    Task<List<PublishedContentListItemResponse>> GetCurrentChallengeContentsAsync(
    CancellationToken cancellationToken = default);

    Task<List<PublishedContentListItemResponse>> GetMyPublishedContentsAsync(
    CancellationToken cancellationToken = default);

    Task<PublishedContentDetailsResponse> GetPublishedContentByIdAsync(
    Guid publishedContentId,
    CancellationToken cancellationToken = default);

    Task<List<ResourceResponse>> GetCurrentChallengeResourcesAsync(
    CancellationToken cancellationToken = default);

    Task<ResourceResponse> GetResourceByIdAsync(
        Guid resourceId,
        CancellationToken cancellationToken = default);

    Task<ResourceResponse> CreateResourceAsync(
    CreateResourceRequest request,
    CancellationToken cancellationToken = default);

    Task<ResourceResponse> UpdateResourceAsync(
        Guid resourceId,
        UpdateResourceRequest request,
        CancellationToken cancellationToken = default);
}