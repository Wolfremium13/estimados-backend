using Common.Estimation.RoomAccess.Domain.Models;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class ParticipantRoleShould
{
    [Theory]
    [InlineData("Moderador", ParticipantRole.Moderador)]
    [InlineData("developer", ParticipantRole.Developer)]
    [InlineData("  Product Owner  ", ParticipantRole.ProductOwner)]
    public void BeCreatedAndNormalizedCorrectlyWithValidRole(string rawRole, string expectedRole)
    {
        var result = ParticipantRole.Create(rawRole);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success => success.Value.ShouldBe(expectedRole));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FailCreationWhenRoleIsMissing(string? invalidRole)
    {
        var result = ParticipantRole.Create(invalidRole);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("ScrumMaster")]
    [InlineData("PO")]
    public void FailCreationWhenRoleIsInvalid(string invalidRole)
    {
        var result = ParticipantRole.Create(invalidRole);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<InvalidRoleException>());
    }

    [Fact]
    public void CorrectlyIdentifyModeratorRole()
    {
        var moderatorRole = ParticipantRole.Create("Moderador").Match(r => r, _ => throw new Exception());
        var developerRole = ParticipantRole.Create("Developer").Match(r => r, _ => throw new Exception());

        moderatorRole.IsModerator.ShouldBeTrue();
        developerRole.IsModerator.ShouldBeFalse();
    }
}