using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence
{
    public class ChallengeDbContext : DbContext
    {
        public ChallengeDbContext(DbContextOptions<ChallengeDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Challenge> Challenges => Set<Challenge>();
        public DbSet<UserChallenge> UserChallenges => Set<UserChallenge>();
        public DbSet<ChallengeDay> ChallengeDays => Set<ChallengeDay>();
        public DbSet<DailyValidation> DailyValidations => Set<DailyValidation>();
        public DbSet<PublishedContent> PublishedContents => Set<PublishedContent>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Resource> Resources => Set<Resource>();
        public DbSet<IdeaGeneration> IdeaGenerations => Set<IdeaGeneration>();
        public DbSet<GeneratedIdea> GeneratedIdeas => Set<GeneratedIdea>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChallengeDbContext).Assembly);
        }
    }
}
