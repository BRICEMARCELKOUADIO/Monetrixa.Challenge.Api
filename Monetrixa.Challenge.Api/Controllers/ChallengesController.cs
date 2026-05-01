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
}