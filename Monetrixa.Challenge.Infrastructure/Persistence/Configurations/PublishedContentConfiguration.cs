using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class PublishedContentConfiguration : IEntityTypeConfiguration<PublishedContent>
    {
        public void Configure(EntityTypeBuilder<PublishedContent> builder)
        {
            builder.ToTable("PublishedContent");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Url)
                .IsRequired();

            builder.Property(x => x.Platform)
                .IsRequired();

            builder.Property(x => x.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(x => x.PublishedAtUtc)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.PublishedContents)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChallengeDay)
                .WithMany(x => x.PublishedContents)
                .HasForeignKey(x => x.ChallengeDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
