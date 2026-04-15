using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class UserChallengeConfiguration : IEntityTypeConfiguration<UserChallenge>
    {
        public void Configure(EntityTypeBuilder<UserChallenge> builder)
        {
            builder.ToTable("UserChallenge");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.JoinedAtUtc)
                .IsRequired();

            builder.Property(x => x.Score)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserChallenges)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Challenge)
                .WithMany(x => x.UserChallenges)
                .HasForeignKey(x => x.ChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.ChallengeId })
                .IsUnique();
        }
    }
}
