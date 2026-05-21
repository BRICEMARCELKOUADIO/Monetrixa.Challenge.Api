using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;

namespace Monetrixa.ChallengeApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaGenerationService _ideaGenerationService;

        public IdeasController(IIdeaGenerationService ideaGenerationService)
        {
            _ideaGenerationService = ideaGenerationService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<IdeaGenerationResponse>> Generate(
            [FromBody] GenerateIdeasRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _ideaGenerationService.GenerateIdeasAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<IdeaGenerationHistoryItemResponse>>> GetHistory(
            CancellationToken cancellationToken)
        {
            var response = await _ideaGenerationService.GetHistoryAsync(cancellationToken);
            return Ok(response);
        }

        [HttpGet("history/{ideaGenerationId:guid}")]
        public async Task<ActionResult<IdeaGenerationResponse>> GetById(
            Guid ideaGenerationId,
            CancellationToken cancellationToken)
        {
            var response = await _ideaGenerationService.GetByIdAsync(ideaGenerationId, cancellationToken);
            return Ok(response);
        }
    }
}
