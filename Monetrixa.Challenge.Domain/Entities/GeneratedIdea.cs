using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class GeneratedIdea
    {
        public Guid Id { get; set; }
        public Guid IdeaGenerationId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public IdeaGeneration IdeaGeneration { get; set; } = null!;
    }
}
