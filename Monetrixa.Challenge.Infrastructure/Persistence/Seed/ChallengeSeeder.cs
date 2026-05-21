using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Domain.Entities;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Seed;

public static class ChallengeSeeder
{
    public static async Task SeedAsync(ChallengeDbContext dbContext, CancellationToken cancellationToken = default)
    {
        const string accessCode = "START2026";

        var existingChallenge = await dbContext.Challenges
            .Include(x => x.Days)
            .FirstOrDefaultAsync(x => x.AccessCode == accessCode, cancellationToken);

        if (existingChallenge is not null)
        {
            return;
        }

        var challengeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(29);

        var challenge = new Challenge
        {
            Id = challengeId,
            Title = "Challenge 30 jours de création de contenu",
            Description = "Un challenge guidé pour aider les participants à produire du contenu régulièrement pendant 30 jours.",
            AccessCode = accessCode,
            StartDateUtc = startDate,
            EndDateUtc = endDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        var days = new List<ChallengeDay>();

        var themes = new[]
        {
            "Se lancer",
            "Clarifier son message",
            "Créer avec régularité",
            "Renforcer sa visibilité",
            "Engager sa communauté",
            "Transformer ses résultats"
        };

        var quotes = new[]
        {
            "L’action crée la clarté.",
            "La constance bat la motivation.",
            "Chaque jour compte.",
            "Le progrès vaut mieux que la perfection.",
            "Publier, c’est apprendre.",
            "La discipline construit les résultats."
        };

        for (var dayNumber = 1; dayNumber <= 30; dayNumber++)
        {
            var weekNumber = ((dayNumber - 1) / 5) + 1;

            var challengeDay = new ChallengeDay
            {
                Id = Guid.NewGuid(),
                ChallengeId = challengeId,
                DayNumber = dayNumber,
                WeekNumber = weekNumber,
                Theme = $"{themes[(weekNumber - 1) % themes.Length]} - Jour {dayNumber}",
                Quote = quotes[(dayNumber - 1) % quotes.Length]
            };

            days.Add(challengeDay);
        }

        dbContext.Challenges.Add(challenge);
        dbContext.ChallengeDays.AddRange(days);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}