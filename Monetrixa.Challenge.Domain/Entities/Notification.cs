using Monetrixa.Challenge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.Challenge.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }

        public User User { get; set; } = null!;
    }
}
