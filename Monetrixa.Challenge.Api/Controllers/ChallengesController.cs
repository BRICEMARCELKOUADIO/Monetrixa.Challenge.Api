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
}