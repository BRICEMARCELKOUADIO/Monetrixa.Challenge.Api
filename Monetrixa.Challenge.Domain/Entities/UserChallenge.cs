using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class UserChallenge
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ChallengeId { get; set; }
        public DateTime JoinedAtUtc { get; set; }
        public int Score { get; set; }

        public User User { get; set; } = null!;
        public Challenge Challenge { get; set; } = null!;
    }
}
