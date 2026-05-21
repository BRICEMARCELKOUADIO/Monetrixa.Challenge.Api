using Monetrixa.ChallengeApp.Domain.Enums;

namespace Monetrixa.ChallengeApp.Application.DTOs.Ideas
{
    public class GenerateIdeasRequest
    {
        public Guid ChallengeDayId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public MoodType Mood { get; set; }
        public PlatformType Platform { get; set; }
    }
}
