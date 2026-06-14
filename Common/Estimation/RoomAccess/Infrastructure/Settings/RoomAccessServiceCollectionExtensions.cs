using Microsoft.Extensions.DependencyInjection;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.RoomAccess.Infrastructure.Repositories;
using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;

namespace Common.Estimation.RoomAccess.Infrastructure.Settings;

public static class RoomAccessServiceCollectionExtensions
{
    public static IServiceCollection AddRoomAccessServices(this IServiceCollection services)
    {
        services.AddSingleton<IEstimationRoomRepository, InMemoryEstimationRoomRepository>();
        
        services.AddTransient<ICreateRoomUseCase, CreateRoomUseCase>();
        services.AddTransient<IRequestToJoinUseCase, RequestToJoinUseCase>();
        services.AddTransient<IApproveJoinRequestUseCase, ApproveJoinRequestUseCase>();
        services.AddTransient<IRejectJoinRequestUseCase, RejectJoinRequestUseCase>();
        services.AddTransient<IDisconnectModeratorUseCase, DisconnectModeratorUseCase>();
        
        return services;
    }
}
