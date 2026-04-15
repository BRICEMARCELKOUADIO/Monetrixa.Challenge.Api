using Monetrixa.ChallengeApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Monetrixa.ChallengeApp.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Participant;
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
        public ICollection<DailyValidation> DailyValidations { get; set; } = new List<DailyValidation>();
        public ICollection<PublishedContent> PublishedContents { get; set; } = new List<PublishedContent>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<IdeaGeneration> IdeaGenerations { get; set; } = new List<IdeaGeneration>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
