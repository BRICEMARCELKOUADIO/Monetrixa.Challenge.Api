using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetrixa.ChallengeApp.Application.DTOs.Challenge;
using Monetrixa.ChallengeApp.Application.Interfaces.Challenge;

namespace Monetrixa.ChallengeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challengeService;

    public ChallengesController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpPost("join")]
    public async Task<ActionResult<ChallengeSummaryResponse>> Join(
        [FromBody] JoinChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _challengeService.JoinChallengeAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("current")]
    public async Task<ActionResult<ChallengeSummaryResponse>> GetCurrent(
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetCurrentChallengeAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("current/days")]
    public async Task<ActionResult<CurrentChallengeDaysResponse>> GetMyChallengeDays(
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetMyChallengeDaysAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("days/validate")]
    public async Task<ActionResult<DailyValidationResponse>> ValidateDay(
    [FromBody] ValidateChallengeDayRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.ValidateChallengeDayAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("contents")]
    public async Task<ActionResult<PublishedContentResponse>> CreatePublishedContent(
    [FromBody] CreatePublishedContentRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.CreatePublishedContentAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("comments")]
    public async Task<ActionResult<CommentResponse>> CreateComment(
    [FromBody] CreateCommentRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.CreateCommentAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("contents/{publishedContentId:guid}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CommentResponse>>> GetComments(
        Guid publishedContentId,
        CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetCommentsByPublishedContentIdAsync(
            publishedContentId,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("contents/current")]
    public async Task<ActionResult<List<PublishedContentListItemResponse>>> GetCurrentChallengeContents(
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetCurrentChallengeContentsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("contents/mine")]
    public async Task<ActionResult<List<PublishedContentListItemResponse>>> GetMyContents(
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetMyPublishedContentsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("contents/{publishedContentId:guid}")]
    public async Task<ActionResult<PublishedContentDetailsResponse>> GetPublishedContentById(
    Guid publishedContentId,
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetPublishedContentByIdAsync(
            publishedContentId,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("resources")]
    public async Task<ActionResult<List<ResourceResponse>>> GetCurrentChallengeResources(
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetCurrentChallengeResourcesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("resources/{resourceId:guid}")]
    public async Task<ActionResult<ResourceResponse>> GetResourceById(
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var response = await _challengeService.GetResourceByIdAsync(resourceId, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("resources")]
    public async Task<ActionResult<ResourceResponse>> CreateResource(
    [FromBody] CreateResourceRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _challengeService.CreateResourceAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("resources/{resourceId:guid}")]
    public async Task<ActionResult<ResourceResponse>> UpdateResource(
        Guid resourceId,
        [FromBody] UpdateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _challengeService.UpdateResourceAsync(resourceId, request, cancellationToken);
        return Ok(response);
    }
}