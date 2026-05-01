using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.DTOs.Challenge
{
    public class ChallengeDayResponse
    {
        public Guid ChallengeDayId { get; set; }
        public int DayNumber { get; set; }
        public int WeekNumber { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string? Quote { get; set; }
        public bool IsValidated { get; set; }
    }
}
