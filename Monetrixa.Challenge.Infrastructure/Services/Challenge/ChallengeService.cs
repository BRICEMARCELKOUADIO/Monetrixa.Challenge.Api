using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Application.DTOs.Challenge;
using Monetrixa.ChallengeApp.Application.Interfaces.Challenge;
using Monetrixa.ChallengeApp.Application.Interfaces.Common;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Infrastructure.Persistence;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Challenge;

public class ChallengeService : IChallengeService
{
    private readonly ChallengeDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ChallengeService(
        ChallengeDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }
     
    public async Task<ChallengeSummaryResponse> JoinChallengeAsync(
        JoinChallengeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AccessCode))
        {
            throw new ArgumentException("Le code d'accès est obligatoire.");
        }

        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var normalizedCode = request.AccessCode.Trim().ToUpperInvariant();

        var challenge = await _dbContext.Challenges
            .FirstOrDefaultAsync(x => x.AccessCode.ToUpper() == normalizedCode, cancellationToken);

        if (challenge is null)
        {
            throw new InvalidOperationException("Challenge introuvable avec ce code.");
        }

        var alreadyJoined = await _dbContext.UserChallenges
            .AnyAsync(
                x => x.UserId == userId.Value && x.ChallengeId == challenge.Id,
                cancellationToken);

        if (alreadyJoined)
        {
            throw new InvalidOperationException("Vous avez déjà rejoint ce challenge.");
        }

        var userChallenge = new UserChallenge
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ChallengeId = challenge.Id,
            JoinedAtUtc = DateTime.UtcNow,
            Score = 0
        };

        _dbContext.UserChallenges.Add(userChallenge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChallengeSummaryResponse
        {
            ChallengeId = challenge.Id,
            Title = challenge.Title,
            AccessCode = challenge.AccessCode,
            Description = challenge.Description,
            StartDateUtc = challenge.StartDateUtc,
            EndDateUtc = challenge.EndDateUtc
        };
    }

    public async Task<ChallengeSummaryResponse> GetCurrentChallengeAsync(
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Include(x => x.Challenge)
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challenge = userChallenge.Challenge;

        return new ChallengeSummaryResponse
        {
            ChallengeId = challenge.Id,
            Title = challenge.Title,
            AccessCode = challenge.AccessCode,
            Description = challenge.Description,
            StartDateUtc = challenge.StartDateUtc,
            EndDateUtc = challenge.EndDateUtc
        };
    }

    public async Task<CurrentChallengeDaysResponse> GetMyChallengeDaysAsync(
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Include(x => x.Challenge)
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challengeId = userChallenge.ChallengeId;

        var validations = await _dbContext.DailyValidations
            .Where(x => x.UserId == userId.Value)
            .Select(x => x.ChallengeDayId)
            .ToListAsync(cancellationToken);

        var days = await _dbContext.ChallengeDays
            .Where(x => x.ChallengeId == challengeId)
            .OrderBy(x => x.DayNumber)
            .Select(x => new ChallengeDayResponse
            {
                ChallengeDayId = x.Id,
                DayNumber = x.DayNumber,
                WeekNumber = x.WeekNumber,
                Theme = x.Theme,
                Quote = x.Quote,
                IsValidated = validations.Contains(x.Id)
            })
            .ToListAsync(cancellationToken);

        return new CurrentChallengeDaysResponse
        {
            ChallengeId = userChallenge.Challenge.Id,
            Title = userChallenge.Challenge.Title,
            Days = days
        };
    }

    public async Task<DailyValidationResponse> ValidateChallengeDayAsync(
    ValidateChallengeDayRequest request,
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        if (request.ChallengeDayId == Guid.Empty)
        {
            throw new ArgumentException("Le jour du challenge est obligatoire.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Include(x => x.Challenge)
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challengeDay = await _dbContext.ChallengeDays
            .FirstOrDefaultAsync(
                x => x.Id == request.ChallengeDayId && x.ChallengeId == userChallenge.ChallengeId,
                cancellationToken);

        if (challengeDay is null)
        {
            throw new InvalidOperationException("Jour de challenge introuvable pour cet utilisateur.");
        }

        var existingValidation = await _dbContext.DailyValidations
            .FirstOrDefaultAsync(
                x => x.UserId == userId.Value && x.ChallengeDayId == request.ChallengeDayId,
                cancellationToken);

        if (existingValidation is not null)
        {
            throw new InvalidOperationException("Cette journée a déjà été validée.");
        }

        var validation = new DailyValidation
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ChallengeDayId = request.ChallengeDayId,
            Status = Domain.Enums.ValidationStatus.Validated,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SubmittedAtUtc = DateTime.UtcNow,
            ValidatedAtUtc = DateTime.UtcNow
        };

        _dbContext.DailyValidations.Add(validation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DailyValidationResponse
        {
            DailyValidationId = validation.Id,
            ChallengeDayId = validation.ChallengeDayId,
            Status = validation.Status.ToString(),
            Note = validation.Note,
            SubmittedAtUtc = validation.SubmittedAtUtc,
            ValidatedAtUtc = validation.ValidatedAtUtc
        };
    }
}