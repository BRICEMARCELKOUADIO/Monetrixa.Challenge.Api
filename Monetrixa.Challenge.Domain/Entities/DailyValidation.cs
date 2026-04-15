using Monetrixa.ChallengeApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Domain.Entities
{
    public class DailyValidation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ChallengeDayId { get; set; }
        public Guid? PublishedContentId { get; set; }

        public ValidationStatus Status { get; set; } = ValidationStatus.Pending;
        public string? Note { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public DateTime? ValidatedAtUtc { get; set; }

        public User User { get; set; } = null!;
        public ChallengeDay ChallengeDay { get; set; } = null!;
        public PublishedContent? PublishedContent { get; set; }
    }
}
