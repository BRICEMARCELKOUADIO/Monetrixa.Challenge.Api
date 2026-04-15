using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.Role)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
