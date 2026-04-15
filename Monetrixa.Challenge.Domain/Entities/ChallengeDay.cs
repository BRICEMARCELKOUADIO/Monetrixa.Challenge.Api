using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Domain.Entities
{
    public class ChallengeDay
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public int DayNumber { get; set; }
        public int WeekNumber { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string? Quote { get; set; }

        public Challenge Challenge { get; set; } = null!;
        public ICollection<DailyValidation> DailyValidations { get; set; } = new List<DailyValidation>();
        public ICollection<PublishedContent> PublishedContents { get; set; } = new List<PublishedContent>();
        public ICollection<IdeaGeneration> IdeaGenerations { get; set; } = new List<IdeaGeneration>();
    }
}
