using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Application.UseCases;
using Common.Estimation.EstimationSession.Domain.Ports;
using Common.Estimation.EstimationSession.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Estimation.EstimationSession.Infrastructure.Settings;

public static class EstimationSessionServiceCollectionExtensions
{
    public static void AddEstimationSessionServices(this IServiceCollection services)
    {
        services.AddSingleton<IEstimationSessionRepository, InMemoryEstimationSessionRepository>();

        services.AddTransient<IStartEstimationSessionUseCase, StartEstimationSessionUseCase>();
        services.AddTransient<ICastVoteUseCase, CastVoteUseCase>();
        services.AddTransient<IRevealVotesUseCase, RevealVotesUseCase>();
        services.AddTransient<IResetVotesUseCase, ResetVotesUseCase>();

        services.AddTransient<ITransitionToPrivateEstimationUseCase, TransitionToPrivateEstimationUseCase>();
        services.AddTransient<ITransitionToConsensusManagementUseCase, TransitionToConsensusManagementUseCase>();
        services.AddTransient<IGetEstimationSessionUseCase, GetEstimationSessionUseCase>();
        services.AddTransient<ICloseEstimationSessionUseCase, CloseEstimationSessionUseCase>();
    }
}