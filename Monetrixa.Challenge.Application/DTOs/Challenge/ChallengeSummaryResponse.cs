namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge;

public class ChallengeSummaryResponse
{
    public Guid ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
}