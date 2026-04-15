using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
    {
        public void Configure(EntityTypeBuilder<Challenge> builder)
        {
            builder.ToTable("Challenge");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.AccessCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.AccessCode)
                .IsUnique();
        }
    }
}
