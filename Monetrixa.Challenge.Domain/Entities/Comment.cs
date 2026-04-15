using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid PublishedContentId { get; set; }
        public Guid UserId { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        public PublishedContent PublishedContent { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
