using Common.Estimation.RoomAccess.Domain.Models;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class RequestIdShould
{
    [Fact]
    public void BeCreatedCorrectlyWithValidGuid()
    {
        var rawGuid = Guid.NewGuid();

        var result = RequestId.Create(rawGuid);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(rawGuid));
    }

    [Fact]
    public void BeCreatedCorrectlyWithValidString()
    {
        var rawGuid = Guid.NewGuid();
        var rawString = rawGuid.ToString();

        var result = RequestId.Create(rawString);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(rawGuid));
    }

    [Fact]
    public void FailCreationWhenGuidIsEmpty()
    {
        var result = RequestId.Create(Guid.Empty);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-guid")]
    public void FailCreationWhenStringIsInvalid(string invalidString)
    {
        var result = RequestId.Create(invalidString);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }
}