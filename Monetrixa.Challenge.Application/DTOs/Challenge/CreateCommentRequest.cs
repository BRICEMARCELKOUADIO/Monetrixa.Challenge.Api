using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class CreateCommentRequest
    {
        public Guid PublishedContentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
