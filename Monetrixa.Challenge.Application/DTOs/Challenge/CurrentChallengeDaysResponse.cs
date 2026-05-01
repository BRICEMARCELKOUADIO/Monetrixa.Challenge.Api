using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class CurrentChallengeDaysResponse
    {
        public Guid ChallengeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<ChallengeDayResponse> Days { get; set; } = new();
    }
}
