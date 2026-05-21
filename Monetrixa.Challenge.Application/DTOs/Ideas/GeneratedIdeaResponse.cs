using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Ideas
{
    public class GeneratedIdeaResponse
    {
        public Guid GeneratedIdeaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
