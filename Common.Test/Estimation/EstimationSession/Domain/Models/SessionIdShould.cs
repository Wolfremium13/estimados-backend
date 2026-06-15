using Common.Estimation.EstimationSession.Domain.Models;
using Shouldly;
using Xunit;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Test.Estimation.EstimationSession.Domain.Models;

public class SessionIdShould
{
    [Fact]
    public void BeCreatedWithValidGuid()
    {
        var guid = Guid.NewGuid();

        var result = SessionId.Create(guid);

        result.IsRight.ShouldBeTrue();
        result.IfRight(id => id.Value.ShouldBe(guid));
    }

    [Fact]
    public void FailWhenGuidIsEmpty()
    {
        var result = SessionId.Create(Guid.Empty);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Fact]
    public void BeCreatedWithValidGuidString()
    {
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        var result = SessionId.Create(guidString);

        result.IsRight.ShouldBeTrue();
        result.IfRight(id => id.Value.ShouldBe(guid));
    }

    [Fact]
    public void FailWhenGuidStringIsInvalid()
    {
        var result = SessionId.Create("not-a-guid");

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }
}