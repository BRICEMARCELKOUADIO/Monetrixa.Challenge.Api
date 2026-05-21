using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class PublishedContentDetailsResponse
    {
        public Guid PublishedContentId { get; set; }
        public Guid ChallengeDayId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime PublishedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int CommentCount { get; set; }
    }
}
