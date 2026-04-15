using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class ChallengeDayConfiguration : IEntityTypeConfiguration<ChallengeDay>
    {
        public void Configure(EntityTypeBuilder<ChallengeDay> builder)
        {
            builder.ToTable("ChallengeDay");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayNumber)
                .IsRequired();

            builder.Property(x => x.WeekNumber)
                .IsRequired();

            builder.Property(x => x.Theme)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Quote)
                .HasMaxLength(500);

            builder.HasOne(x => x.Challenge)
                .WithMany(x => x.Days)
                .HasForeignKey(x => x.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ChallengeId, x.DayNumber })
                .IsUnique();
        }
    }
}
