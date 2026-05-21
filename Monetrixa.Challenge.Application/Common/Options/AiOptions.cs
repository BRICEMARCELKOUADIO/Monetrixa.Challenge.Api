using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Application.Common.Options
{
    public class AiOptions
    {
        public const string SectionName = "Ai";

        public string Provider { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
