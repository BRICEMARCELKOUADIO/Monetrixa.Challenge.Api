using Monetrixa.ChallengeApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class CreatePublishedContentRequest
    {
        public Guid ChallengeDayId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public PlatformType Platform { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime PublishedAtUtc { get; set; }
    }
}
