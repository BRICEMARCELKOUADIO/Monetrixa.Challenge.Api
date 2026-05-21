using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Domain.Enums;

namespace Monetrixa.ChallengeApp.Infrastructure.Persistence.Seed
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAsync(ChallengeDbContext dbContext, CancellationToken cancellationToken = default)
        {
            const string adminEmail = "admin@monetrixa.com";
            const string adminPassword = "Admin@12345";

            var existingAdmin = await dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

            if (existingAdmin is not null)
            {
                return;
            }

            var admin = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Monetrixa Admin",
                Email = adminEmail,
                Role = UserRole.Admin,
                CreatedAtUtc = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<User>();
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
