using Common.Estimation.RoomAccess.Domain.Models;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class ParticipantNameShould
{
    [Theory]
    [InlineData("Carlos")]
    [InlineData("Ana Developer")]
    [InlineData("  Carlos  ")]
    public void BeCreatedCorrectlyWithValidName(string validName)
    {
        var result = ParticipantName.Create(validName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(validName.Trim()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FailCreationWhenNameIsInvalid(string? invalidName)
    {
        var result = ParticipantName.Create(invalidName);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }
}