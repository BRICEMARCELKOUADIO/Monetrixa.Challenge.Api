using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class Challenge
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
        public ICollection<ChallengeDay> Days { get; set; } = new List<ChallengeDay>();
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    }
}
