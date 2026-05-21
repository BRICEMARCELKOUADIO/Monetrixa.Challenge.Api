using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Ideas
{
    public class IdeaGenerationHistoryItemResponse
    {
        public Guid IdeaGenerationId { get; set; }
        public Guid ChallengeDayId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Mood { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public int IdeasCount { get; set; }
    }
}
