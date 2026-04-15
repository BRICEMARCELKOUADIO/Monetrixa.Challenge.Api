using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class DailyValidationConfiguration : IEntityTypeConfiguration<DailyValidation>
    {
        public void Configure(EntityTypeBuilder<DailyValidation> builder)
        {
            builder.ToTable("DailyValidation");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(1000);

            builder.HasOne(x => x.User)
                .WithMany(x => x.DailyValidations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChallengeDay)
                .WithMany(x => x.DailyValidations)
                .HasForeignKey(x => x.ChallengeDayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PublishedContent)
                .WithMany(x => x.DailyValidations)
                .HasForeignKey(x => x.PublishedContentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.UserId, x.ChallengeDayId })
                .IsUnique();
        }
    }
}
