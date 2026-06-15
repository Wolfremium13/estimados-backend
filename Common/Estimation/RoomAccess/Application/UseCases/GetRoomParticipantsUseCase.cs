using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class GetRoomParticipantsUseCase(IEstimationRoomRepository roomRepository) : IGetRoomParticipantsUseCase
{
    public async Task<Either<Error, IReadOnlyCollection<ParticipantDto>>> Execute(GetRoomParticipantsQuery query)
    {
        return await (
            from rId in RoomId.Create(query.RoomId).ToAsync()
            from room in roomRepository.FindById(rId).ToAsync()
            select (IReadOnlyCollection<ParticipantDto>)room.ActiveParticipants
                .Select(p => new ParticipantDto(p.Name.Value, p.Role.Value))
                .ToList()
                .AsReadOnly()
        );
    }
}