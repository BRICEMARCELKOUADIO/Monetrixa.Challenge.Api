using Monetrixa.ChallengeApp.Application.DTOs.Auth;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Domain.Enums;
using Monetrixa.ChallengeApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Application.Interfaces.Auth;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ChallengeDbContext _dbContext;
        private readonly PasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            ChallengeDbContext dbContext,
            PasswordService passwordService,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

            if (existingUser is not null)
            {
                throw new InvalidOperationException("Un compte existe déjà avec cet email.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            };

            user.PasswordHash = _passwordService.HashPassword(user, request.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                ExpiresAtUtc = _jwtTokenService.GetExpirationUtc(),
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

            if (user is null)
            {
                throw new UnauthorizedAccessException("Email ou mot de passe invalide.");
            }

            var isValidPassword = _passwordService.VerifyPassword(user, user.PasswordHash, request.Password);

            if (!isValidPassword)
            {
                throw new UnauthorizedAccessException("Email ou mot de passe invalide.");
            }

            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                ExpiresAtUtc = _jwtTokenService.GetExpirationUtc(),
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }
    }
}
