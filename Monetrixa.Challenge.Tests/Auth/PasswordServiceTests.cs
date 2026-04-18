using FluentAssertions;
using Monetrixa.ChallengeApp.Domain.Entities;
using Monetrixa.ChallengeApp.Infrastructure.Services.Auth;

namespace Monetrixa.ChallengeApp.Tests.Auth;

public class PasswordServiceTests
{
    [Fact]
    public void HashPassword_Should_Return_A_Hash()
    {
        var service = new PasswordService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Brice",
            Email = "brice@test.com"
        };

        var hash = service.HashPassword(user, "Brice@12345");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe("Brice@12345");
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_When_Password_Is_Valid()
    {
        var service = new PasswordService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Brice",
            Email = "brice@test.com"
        };

        var hash = service.HashPassword(user, "Brice@12345");

        var result = service.VerifyPassword(user, hash, "Brice@12345");

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_When_Password_Is_Invalid()
    {
        var service = new PasswordService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Brice",
            Email = "brice@test.com"
        };

        var hash = service.HashPassword(user, "Brice@12345");

        var result = service.VerifyPassword(user, hash, "WrongPassword");

        result.Should().BeFalse();
    }
}