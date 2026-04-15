using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetrixa.ChallengeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.UserId, x.IsRead });
        }
    }
}
