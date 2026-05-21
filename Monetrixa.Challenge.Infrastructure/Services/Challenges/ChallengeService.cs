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

    public async Task<PublishedContentResponse> CreatePublishedContentAsync(
    CreatePublishedContentRequest request,
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

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Le titre est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new ArgumentException("L'URL du contenu est obligatoire.");
        }

        var userChallenge = await _dbContext.UserChallenges
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

        var publishedContent = new PublishedContent
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ChallengeDayId = request.ChallengeDayId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Url = request.Url.Trim(),
            Platform = request.Platform,
            ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl.Trim(),
            PublishedAtUtc = request.PublishedAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.PublishedContents.Add(publishedContent);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PublishedContentResponse
        {
            PublishedContentId = publishedContent.Id,
            ChallengeDayId = publishedContent.ChallengeDayId,
            Title = publishedContent.Title,
            Description = publishedContent.Description,
            Url = publishedContent.Url,
            Platform = publishedContent.Platform.ToString(),
            ThumbnailUrl = publishedContent.ThumbnailUrl,
            PublishedAtUtc = publishedContent.PublishedAtUtc,
            CreatedAtUtc = publishedContent.CreatedAtUtc
        };
    }

    public async Task<CommentResponse> CreateCommentAsync(
    CreateCommentRequest request,
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        if (request.PublishedContentId == Guid.Empty)
        {
            throw new ArgumentException("Le contenu publié est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Le commentaire est obligatoire.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Utilisateur introuvable.");
        }

        var publishedContent = await _dbContext.PublishedContents
            .FirstOrDefaultAsync(x => x.Id == request.PublishedContentId, cancellationToken);

        if (publishedContent is null)
        {
            throw new InvalidOperationException("Contenu publié introuvable.");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PublishedContentId = request.PublishedContentId,
            UserId = userId.Value,
            Content = request.Content.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CommentResponse
        {
            CommentId = comment.Id,
            PublishedContentId = comment.PublishedContentId,
            UserId = comment.UserId,
            UserFullName = user.FullName,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc
        };
    }

    public async Task<List<CommentResponse>> GetCommentsByPublishedContentIdAsync(
        Guid publishedContentId,
        CancellationToken cancellationToken = default)
    {
        if (publishedContentId == Guid.Empty)
        {
            throw new ArgumentException("Le contenu publié est obligatoire.");
        }

        var publishedContentExists = await _dbContext.PublishedContents
            .AnyAsync(x => x.Id == publishedContentId, cancellationToken);

        if (!publishedContentExists)
        {
            throw new InvalidOperationException("Contenu publié introuvable.");
        }

        var comments = await _dbContext.Comments
            .Where(x => x.PublishedContentId == publishedContentId)
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new CommentResponse
            {
                CommentId = x.Id,
                PublishedContentId = x.PublishedContentId,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return comments;
    }

    public async Task<List<PublishedContentListItemResponse>> GetCurrentChallengeContentsAsync(
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challengeId = userChallenge.ChallengeId;

        var contents = await _dbContext.PublishedContents
            .Where(x => x.ChallengeDay.ChallengeId == challengeId)
            .Include(x => x.User)
            .Include(x => x.Comments)
            .OrderByDescending(x => x.PublishedAtUtc)
            .Select(x => new PublishedContentListItemResponse
            {
                PublishedContentId = x.Id,
                ChallengeDayId = x.ChallengeDayId,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                Platform = x.Platform.ToString(),
                ThumbnailUrl = x.ThumbnailUrl,
                PublishedAtUtc = x.PublishedAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                CommentCount = x.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return contents;
    }

    public async Task<List<PublishedContentListItemResponse>> GetMyPublishedContentsAsync(
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challengeId = userChallenge.ChallengeId;

        var contents = await _dbContext.PublishedContents
            .Where(x => x.UserId == userId.Value && x.ChallengeDay.ChallengeId == challengeId)
            .Include(x => x.User)
            .Include(x => x.Comments)
            .OrderByDescending(x => x.PublishedAtUtc)
            .Select(x => new PublishedContentListItemResponse
            {
                PublishedContentId = x.Id,
                ChallengeDayId = x.ChallengeDayId,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                Platform = x.Platform.ToString(),
                ThumbnailUrl = x.ThumbnailUrl,
                PublishedAtUtc = x.PublishedAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                CommentCount = x.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return contents;
    }

    public async Task<PublishedContentDetailsResponse> GetPublishedContentByIdAsync(
    Guid publishedContentId,
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        if (publishedContentId == Guid.Empty)
        {
            throw new ArgumentException("Le contenu publié est obligatoire.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var challengeId = userChallenge.ChallengeId;

        var content = await _dbContext.PublishedContents
            .Where(x => x.Id == publishedContentId && x.ChallengeDay.ChallengeId == challengeId)
            .Include(x => x.User)
            .Include(x => x.Comments)
            .Select(x => new PublishedContentDetailsResponse
            {
                PublishedContentId = x.Id,
                ChallengeDayId = x.ChallengeDayId,
                UserId = x.UserId,
                UserFullName = x.User.FullName,
                Title = x.Title,
                Description = x.Description,
                Url = x.Url,
                Platform = x.Platform.ToString(),
                ThumbnailUrl = x.ThumbnailUrl,
                PublishedAtUtc = x.PublishedAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                CommentCount = x.Comments.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (content is null)
        {
            throw new InvalidOperationException("Contenu publié introuvable pour cet utilisateur.");
        }

        return content;
    }

    public async Task<List<ResourceResponse>> GetCurrentChallengeResourcesAsync(
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var resources = await _dbContext.Resources
            .Where(x => x.ChallengeId == userChallenge.ChallengeId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .Select(x => new ResourceResponse
            {
                ResourceId = x.Id,
                ChallengeId = x.ChallengeId,
                Title = x.Title,
                Description = x.Description,
                ResourceType = x.ResourceType.ToString(),
                Url = x.Url,
                FileName = x.FileName,
                ContentType = x.ContentType,
                ThumbnailUrl = x.ThumbnailUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return resources;
    }

    public async Task<ResourceResponse> GetResourceByIdAsync(
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("La ressource est obligatoire.");
        }

        var userChallenge = await _dbContext.UserChallenges
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.JoinedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChallenge is null)
        {
            throw new InvalidOperationException("Aucun challenge rejoint pour cet utilisateur.");
        }

        var resource = await _dbContext.Resources
            .Where(x => x.Id == resourceId
                        && x.ChallengeId == userChallenge.ChallengeId
                        && x.IsActive)
            .Select(x => new ResourceResponse
            {
                ResourceId = x.Id,
                ChallengeId = x.ChallengeId,
                Title = x.Title,
                Description = x.Description,
                ResourceType = x.ResourceType.ToString(),
                Url = x.Url,
                FileName = x.FileName,
                ContentType = x.ContentType,
                ThumbnailUrl = x.ThumbnailUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (resource is null)
        {
            throw new InvalidOperationException("Ressource introuvable pour cet utilisateur.");
        }

        return resource;
    }

    public async Task<ResourceResponse> CreateResourceAsync(
    CreateResourceRequest request,
    CancellationToken cancellationToken = default)
    {
        if (request.ChallengeId == Guid.Empty)
        {
            throw new ArgumentException("Le challenge est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Le titre est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new ArgumentException("L'URL est obligatoire.");
        }

        var challengeExists = await _dbContext.Challenges
            .AnyAsync(x => x.Id == request.ChallengeId, cancellationToken);

        if (!challengeExists)
        {
            throw new InvalidOperationException("Challenge introuvable.");
        }

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            ChallengeId = request.ChallengeId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ResourceType = request.ResourceType,
            Url = request.Url.Trim(),
            FileName = string.IsNullOrWhiteSpace(request.FileName) ? null : request.FileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim(),
            ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Resources.Add(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResourceResponse
        {
            ResourceId = resource.Id,
            ChallengeId = resource.ChallengeId,
            Title = resource.Title,
            Description = resource.Description,
            ResourceType = resource.ResourceType.ToString(),
            Url = resource.Url,
            FileName = resource.FileName,
            ContentType = resource.ContentType,
            ThumbnailUrl = resource.ThumbnailUrl,
            DisplayOrder = resource.DisplayOrder,
            IsActive = resource.IsActive,
            CreatedAtUtc = resource.CreatedAtUtc
        };
    }

    public async Task<ResourceResponse> UpdateResourceAsync(
        Guid resourceId,
        UpdateResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("La ressource est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Le titre est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new ArgumentException("L'URL est obligatoire.");
        }

        var resource = await _dbContext.Resources
            .FirstOrDefaultAsync(x => x.Id == resourceId, cancellationToken);

        if (resource is null)
        {
            throw new InvalidOperationException("Ressource introuvable.");
        }

        resource.Title = request.Title.Trim();
        resource.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        resource.ResourceType = request.ResourceType;
        resource.Url = request.Url.Trim();
        resource.FileName = string.IsNullOrWhiteSpace(request.FileName) ? null : request.FileName.Trim();
        resource.ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim();
        resource.ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl.Trim();
        resource.DisplayOrder = request.DisplayOrder;
        resource.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResourceResponse
        {
            ResourceId = resource.Id,
            ChallengeId = resource.ChallengeId,
            Title = resource.Title,
            Description = resource.Description,
            ResourceType = resource.ResourceType.ToString(),
            Url = resource.Url,
            FileName = resource.FileName,
            ContentType = resource.ContentType,
            ThumbnailUrl = resource.ThumbnailUrl,
            DisplayOrder = resource.DisplayOrder,
            IsActive = resource.IsActive,
            CreatedAtUtc = resource.CreatedAtUtc
        };
    }
}