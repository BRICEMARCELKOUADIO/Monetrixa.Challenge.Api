using System.Security.Claims;

namespace Monetrixa.ChallengeApp.Application.Interfaces.Common;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    string? Role { get; }
}