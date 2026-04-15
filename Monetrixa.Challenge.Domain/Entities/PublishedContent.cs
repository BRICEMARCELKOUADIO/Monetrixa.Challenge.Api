using Monetrixa.Challenge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class PublishedContent
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ChallengeDayId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public PlatformType Platform { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime PublishedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public User User { get; set; } = null!;
        public ChallengeDay ChallengeDay { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<DailyValidation> DailyValidations { get; set; } = new List<DailyValidation>();
    }
}
