using Monetrixa.Challenge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class Resource
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ResourceType ResourceType { get; set; }

        public string Url { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? ThumbnailUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }

        public Challenge Challenge { get; set; } = null!;
    }
}
