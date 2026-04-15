using Monetrixa.Challenge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class IdeaGeneration
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ChallengeDayId { get; set; }

        public string Topic { get; set; } = string.Empty;
        public MoodType Mood { get; set; }
        public PlatformType Platform { get; set; }
        public string PromptUsed { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        public User User { get; set; } = null!;
        public ChallengeDay ChallengeDay { get; set; } = null!;
        public ICollection<GeneratedIdea> GeneratedIdeas { get; set; } = new List<GeneratedIdea>();
    }
}
