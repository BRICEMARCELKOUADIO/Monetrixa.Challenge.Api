using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class DailyValidationResponse
    {
        public Guid DailyValidationId { get; set; }
        public Guid ChallengeDayId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public DateTime? ValidatedAtUtc { get; set; }
    }
}
