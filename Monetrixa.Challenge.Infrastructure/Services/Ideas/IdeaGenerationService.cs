using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Application.Interfaces.Common;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Infrastructure.Persistence;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Ideas;

public class IdeaGenerationService : IIdeaGenerationService
{
    private readonly ChallengeDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAiProviderFactory _aiProviderFactory;
    private readonly AiOptions _aiOptions;

    public IdeaGenerationService(
        ChallengeDbContext dbContext,
        ICurrentUserService currentUserService,
        IAiProviderFactory aiProviderFactory,
        IOptions<AiOptions> aiOptions)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _aiProviderFactory = aiProviderFactory;
        _aiOptions = aiOptions.Value;
    }

    public async Task<IdeaGenerationResponse> GenerateIdeasAsync(
        GenerateIdeasRequest request,
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

        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            throw new ArgumentException("Le sujet est obligatoire.");
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

        if (!Enum.TryParse<AiProvider>(_aiOptions.Provider, true, out var providerType))
        {
            throw new InvalidOperationException($"Provider IA invalide : {_aiOptions.Provider}");
        }

        var provider = _aiProviderFactory.GetProvider(providerType);

        var promptUsed =
            $"Topic: {request.Topic}; Mood: {request.Mood}; Platform: {request.Platform}; Theme: {challengeDay.Theme}; Quote: {challengeDay.Quote}";

        var aiIdeas = await provider.GenerateIdeasAsync(
            request.Topic.Trim(),
            request.Mood,
            request.Platform,
            challengeDay.Theme,
            challengeDay.Quote,
            cancellationToken);

        var generation = new IdeaGeneration
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ChallengeDayId = request.ChallengeDayId,
            Topic = request.Topic.Trim(),
            Mood = request.Mood,
            Platform = request.Platform,
            PromptUsed = promptUsed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.IdeaGenerations.Add(generation);

        var generatedIdeas = aiIdeas
            .Select((idea, index) => new GeneratedIdea
            {
                Id = Guid.NewGuid(),
                IdeaGenerationId = generation.Id,
                Title = idea.Title,
                Content = idea.Content,
                DisplayOrder = index + 1
            })
            .ToList();

        _dbContext.GeneratedIdeas.AddRange(generatedIdeas);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IdeaGenerationResponse
        {
            IdeaGenerationId = generation.Id,
            ChallengeDayId = generation.ChallengeDayId,
            Topic = generation.Topic,
            Mood = generation.Mood.ToString(),
            Platform = generation.Platform.ToString(),
            CreatedAtUtc = generation.CreatedAtUtc,
            Ideas = generatedIdeas
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GeneratedIdeaResponse
                {
                    GeneratedIdeaId = x.Id,
                    Title = x.Title,
                    Content = x.Content,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };
    }

    public async Task<List<IdeaGenerationHistoryItemResponse>> GetHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        return await _dbContext.IdeaGenerations
            .Where(x => x.UserId == userId.Value)
            .Include(x => x.GeneratedIdeas)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new IdeaGenerationHistoryItemResponse
            {
                IdeaGenerationId = x.Id,
                ChallengeDayId = x.ChallengeDayId,
                Topic = x.Topic,
                Mood = x.Mood.ToString(),
                Platform = x.Platform.ToString(),
                CreatedAtUtc = x.CreatedAtUtc,
                IdeasCount = x.GeneratedIdeas.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IdeaGenerationResponse> GetByIdAsync(
        Guid ideaGenerationId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié.");
        }

        if (ideaGenerationId == Guid.Empty)
        {
            throw new ArgumentException("La génération d'idées est obligatoire.");
        }

        var generation = await _dbContext.IdeaGenerations
            .Include(x => x.GeneratedIdeas)
            .FirstOrDefaultAsync(
                x => x.Id == ideaGenerationId && x.UserId == userId.Value,
                cancellationToken);

        if (generation is null)
        {
            throw new InvalidOperationException("Historique de génération introuvable.");
        }

        return new IdeaGenerationResponse
        {
            IdeaGenerationId = generation.Id,
            ChallengeDayId = generation.ChallengeDayId,
            Topic = generation.Topic,
            Mood = generation.Mood.ToString(),
            Platform = generation.Platform.ToString(),
            CreatedAtUtc = generation.CreatedAtUtc,
            Ideas = generation.GeneratedIdeas
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GeneratedIdeaResponse
                {
                    GeneratedIdeaId = x.Id,
                    Title = x.Title,
                    Content = x.Content,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };
    }
}