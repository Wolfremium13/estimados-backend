using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Test.Estimation.RoomAccess.Domain.Builders;
using LanguageExt.Common;
using NSubstitute;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class GetPendingJoinRequestsUseCaseShould
{
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly GetPendingJoinRequestsUseCase _useCase;

    public GetPendingJoinRequestsUseCaseShould()
    {
        _useCase = new GetPendingJoinRequestsUseCase(_repository);
    }

    [Fact]
    public async Task ReturnPendingJoinRequestsSuccessfully()
    {
        var request1 = new JoinRequestBuilder().WithName("Ana").WithRole(ParticipantRole.Developer).Build();
        var request2 = new JoinRequestBuilder().WithName("Pedro").WithRole(ParticipantRole.ProductOwner).Build();

        var room = new EstimationRoomBuilder()
            .WithId(_roomId)
            .WithJoinRequest(request1)
            .WithJoinRequest(request2)
            .Build();

        _repository.FindById(Arg.Any<RoomId>()).Returns(room);

        var result = await _useCase.Execute(new GetPendingJoinRequestsQuery(_roomId));

        result.IsRight.ShouldBeTrue();
        result.IfRight(dtos =>
        {
            dtos.Count.ShouldBe(2);
            dtos.ShouldContain(d => d.Name == "Ana" && d.Role == ParticipantRole.Developer);
            dtos.ShouldContain(d => d.Name == "Pedro" && d.Role == ParticipantRole.ProductOwner);
        });
    }

    [Fact]
    public async Task FailWhenRoomNotFound()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(Error.New(new RoomNotFoundException("Not found")));

        var result = await _useCase.Execute(new GetPendingJoinRequestsQuery(_roomId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => { error.ToException().ShouldBeOfType<RoomNotFoundException>(); });
    }

    [Fact]
    public async Task FailWhenRoomIsInactive()
    {
        var room = new EstimationRoomBuilder()
            .WithId(_roomId)
            .AsClosed()
            .Build();

        _repository.FindById(Arg.Any<RoomId>()).Returns(room);

        var result = await _useCase.Execute(new GetPendingJoinRequestsQuery(_roomId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            error.Message.ShouldContain("is not active");
        });
    }
}