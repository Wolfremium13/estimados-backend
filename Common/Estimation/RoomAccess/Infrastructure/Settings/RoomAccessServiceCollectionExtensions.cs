using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.RoomAccess.Infrastructure.Adapters;
using Common.Estimation.RoomAccess.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Estimation.RoomAccess.Infrastructure.Settings;

public static class RoomAccessServiceCollectionExtensions
{
    public static void AddRoomAccessServices(this IServiceCollection services)
    {
        services.AddSingleton<IEstimationRoomRepository, InMemoryEstimationRoomRepository>();
        services.AddSingleton<IGuidGenerator, SystemGuidGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddTransient<ICreateRoomUseCase, CreateRoomUseCase>();
        services.AddTransient<IRequestToJoinUseCase, RequestToJoinUseCase>();
        services.AddTransient<IApproveJoinRequestUseCase, ApproveJoinRequestUseCase>();
        services.AddTransient<IRejectJoinRequestUseCase, RejectJoinRequestUseCase>();
        services.AddTransient<IDisconnectModeratorUseCase, DisconnectModeratorUseCase>();
        services.AddTransient<IGetRoomParticipantsUseCase, GetRoomParticipantsUseCase>();
    }
}