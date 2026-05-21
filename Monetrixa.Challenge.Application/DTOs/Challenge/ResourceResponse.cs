using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class ResourceResponse
    {
        public Guid ResourceId { get; set; }
        public Guid ChallengeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
