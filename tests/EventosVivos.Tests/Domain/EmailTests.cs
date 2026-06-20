using EventosVivos.Domain.Common;
using EventosVivos.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Tests.Domain;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@test.com")]
    [InlineData("john.doe@company.co")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        Result<Email> result = Email.Create(email);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nolocal.com")]
    public void Create_WithInvalidEmail_ShouldFail(string email)
    {
        Result<Email> result = Email.Create(email);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldBeCaseInsensitive()
    {
        Email a = Email.Create("User@Test.com").Value;
        Email b = Email.Create("user@test.com").Value;

        a.Should().Be(b);
    }
}
