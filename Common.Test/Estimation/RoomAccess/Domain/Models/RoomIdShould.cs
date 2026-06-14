using System;
using Xunit;
using Shouldly;
using Common.Estimation.RoomAccess.Domain.Models;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class RoomIdShould
{
    [Fact]
    public void BeCreatedCorrectlyWithValidGuid()
    {
        var rawGuid = Guid.NewGuid();

        var result = RoomId.Create(rawGuid);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(rawGuid));
    }

    [Fact]
    public void BeCreatedCorrectlyWithValidString()
    {
        var rawGuid = Guid.NewGuid();
        var rawString = rawGuid.ToString();

        var result = RoomId.Create(rawString);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(rawGuid));
    }

    [Fact]
    public void FailCreationWhenGuidIsEmpty()
    {
        var result = RoomId.Create(Guid.Empty);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-guid")]
    public void FailCreationWhenStringIsInvalid(string invalidString)
    {
        var result = RoomId.Create(invalidString);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }
}
