using System.Net;
using System.Net.Http.Json;
using Common.Estimation.RoomAccess.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Xunit;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.E2E;

[Collection("ScenarioTests")]
public class RoomAccessBddScenariosShould : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public RoomAccessBddScenariosShould(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        InMemoryEstimationRoomRepository.Clear();
    }

    public void Dispose()
    {
        InMemoryEstimationRoomRepository.Clear();
    }

    [Fact]
    public async Task Scenario1_ModeratorEntersWebAndCreatesNewRoom()
    {
        var response = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RoomCreateResponse>();
        content.ShouldNotBeNull();
        content.RoomId.ShouldNotBe(Guid.Empty);
        content.ModeratorName.ShouldBe("Carlos");

        await using var hubConnection = CreateHubConnection(content.RoomId);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinRoomAsModerator", content.RoomId);
    }

    [Fact]
    public async Task Scenario2_DeveloperOrProductOwnerJoinsRoomAndIsApprovedByModerator()
    {
        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        await using var hubConnectionCarlos = CreateHubConnection(roomId);
        await hubConnectionCarlos.StartAsync();
        await hubConnectionCarlos.InvokeAsync("JoinRoomAsModerator", roomId);

        var requestReceivedTask = new TaskCompletionSource<(Guid RequestId, string Name, string Role)>();
        hubConnectionCarlos.On<Guid, string, string>("OnJoinRequestReceived",
            (reqId, name, role) => { requestReceivedTask.SetResult((reqId, name, role)); });

        var joinPayload = new { name = "Ana", role = "Developer" };
        var joinResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", joinPayload);
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var requestData = await joinResponse.Content.ReadFromJsonAsync<RoomJoinRequestResponse>();
        var requestId = requestData!.RequestId;

        await using var hubConnectionAna = CreateHubConnection(roomId);
        await hubConnectionAna.StartAsync();
        await hubConnectionAna.InvokeAsync("JoinRoomAsParticipant", roomId);

        var requestApprovedTask = new TaskCompletionSource<Guid>();
        hubConnectionAna.On<Guid>("OnJoinRequestApproved", reqId => { requestApprovedTask.SetResult(reqId); });

        var receivedData = await requestReceivedTask.Task;
        receivedData.RequestId.ShouldBe(requestId);
        receivedData.Name.ShouldBe("Ana");
        receivedData.Role.ShouldBe("Developer");

        var approveResponse = await _httpClient.PostAsync($"v1/rooms/{roomId}/join-requests/{requestId}/approve", null);
        approveResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var approvedId = await requestApprovedTask.Task;
        approvedId.ShouldBe(requestId);
    }

    [Fact]
    public async Task Scenario3_UserHasSavedNameAndDecidesToChangeIt()
    {
        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        await using var hubConnectionCarlos = CreateHubConnection(roomId);
        await hubConnectionCarlos.StartAsync();
        await hubConnectionCarlos.InvokeAsync("JoinRoomAsModerator", roomId);

        var requestReceivedTask = new TaskCompletionSource<(Guid RequestId, string Name, string Role)>();
        hubConnectionCarlos.On<Guid, string, string>("OnJoinRequestReceived",
            (reqId, name, role) => { requestReceivedTask.SetResult((reqId, name, role)); });

        var joinPayload = new { name = "Ana Developer", role = "Developer" };
        var joinResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", joinPayload);
        var requestData = await joinResponse.Content.ReadFromJsonAsync<RoomJoinRequestResponse>();
        var requestId = requestData!.RequestId;

        var receivedData = await requestReceivedTask.Task;
        receivedData.RequestId.ShouldBe(requestId);
        receivedData.Name.ShouldBe("Ana Developer");
    }

    [Fact]
    public async Task Scenario4_DeveloperIntroducesInvalidOrInactiveRoomUuid()
    {
        var invalidRoomId = Guid.NewGuid();
        var joinPayload = new { name = "Ana", role = "Developer" };

        var joinResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{invalidRoomId}/join-requests", joinPayload);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Scenario5_ModeratorRejectsAccessRequest()
    {
        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        await using var hubConnectionCarlos = CreateHubConnection(roomId);
        await hubConnectionCarlos.StartAsync();
        await hubConnectionCarlos.InvokeAsync("JoinRoomAsModerator", roomId);

        var requestReceivedTask = new TaskCompletionSource<Guid>();
        hubConnectionCarlos.On<Guid, string, string>("OnJoinRequestReceived",
            (reqId, _, _) => { requestReceivedTask.SetResult(reqId); });

        var joinPayload = new { name = "Bob", role = "Product Owner" };
        var joinResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", joinPayload);
        var requestData = await joinResponse.Content.ReadFromJsonAsync<RoomJoinRequestResponse>();
        var requestId = requestData!.RequestId;

        await using var hubConnectionBob = CreateHubConnection(roomId);
        await hubConnectionBob.StartAsync();
        await hubConnectionBob.InvokeAsync("JoinRoomAsParticipant", roomId);

        var requestRejectedTask = new TaskCompletionSource<Guid>();
        hubConnectionBob.On<Guid>("OnJoinRequestRejected", reqId => { requestRejectedTask.SetResult(reqId); });

        var receivedId = await requestReceivedTask.Task;
        receivedId.ShouldBe(requestId);

        var rejectResponse = await _httpClient.PostAsync($"v1/rooms/{roomId}/join-requests/{requestId}/reject", null);
        rejectResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var rejectedId = await requestRejectedTask.Task;
        rejectedId.ShouldBe(requestId);
    }

    [Fact]
    public async Task Scenario6_AttemptToAdvanceWithoutCompletingRequiredFields()
    {
        var responseEmptyModerator = await _httpClient.PostAsync("v1/rooms?moderatorName=", null);
        var responseWhitespaceModerator = await _httpClient.PostAsync("v1/rooms?moderatorName=%20%20", null);

        responseEmptyModerator.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        responseWhitespaceModerator.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        var payloadEmptyName = new { name = "", role = "Developer" };
        var responseEmptyName = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", payloadEmptyName);

        var payloadEmptyRole = new { name = "Ana", role = "" };
        var responseEmptyRole = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", payloadEmptyRole);

        var payloadInvalidRole = new { name = "Ana", role = "Manager" };
        var responseInvalidRole =
            await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", payloadInvalidRole);

        responseEmptyName.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        responseEmptyRole.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        responseInvalidRole.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Scenario7_ModeratorDisconnectsBeforeApprovingUser()
    {
        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        var hubConnectionCarlos = CreateHubConnection(roomId);
        await hubConnectionCarlos.StartAsync();
        await hubConnectionCarlos.InvokeAsync("JoinRoomAsModerator", roomId);

        var joinPayload = new { name = "Ana", role = "Developer" };
        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", joinPayload);

        await using var hubConnectionAna = CreateHubConnection(roomId);
        await hubConnectionAna.StartAsync();
        await hubConnectionAna.InvokeAsync("JoinRoomAsParticipant", roomId);

        var roomClosedTask = new TaskCompletionSource<bool>();
        hubConnectionAna.On("OnRoomClosed", () => { roomClosedTask.SetResult(true); });

        await hubConnectionCarlos.StopAsync();
        await hubConnectionCarlos.DisposeAsync();

        var roomClosedTriggered = await roomClosedTask.Task;
        roomClosedTriggered.ShouldBeTrue();
    }

    [Fact]
    public async Task Scenario8_SecondModeratorConnectionIsRejected()
    {
        var response = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var content = await response.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = content!.RoomId;

        // First connection as moderator
        await using var hubConnection1 = CreateHubConnection(roomId);
        await hubConnection1.StartAsync();
        await hubConnection1.InvokeAsync("JoinRoomAsModerator", roomId);

        // Second connection as moderator
        await using var hubConnection2 = CreateHubConnection(roomId);
        await hubConnection2.StartAsync();

        // Attempting to join should throw a HubException
        var exception = await Assert.ThrowsAsync<HubException>(async () =>
        {
            await hubConnection2.InvokeAsync("JoinRoomAsModerator", roomId);
        });

        exception.Message.ShouldContain("A moderator is already connected to this room.");
    }

    private HubConnection CreateHubConnection(Guid roomId)
    {
        return new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/room", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();
    }
}