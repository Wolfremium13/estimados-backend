using System.Net;
using System.Net.Http.Json;
using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Infrastructure.Persistence;
using Common.Estimation.RoomAccess.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Xunit;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.E2E;

[Collection("ScenarioTests")]
public class EstimationSessionBddScenariosShould : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public EstimationSessionBddScenariosShould(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        InMemoryEstimationRoomRepository.Clear();
        InMemoryEstimationSessionRepository.Clear();
    }

    public void Dispose()
    {
        InMemoryEstimationRoomRepository.Clear();
        InMemoryEstimationSessionRepository.Clear();
    }

    [Fact]
    public async Task Scenario8_DeveloperCastsValidStoryPointVote()
    {
        var roomId = await SetupRoomWithParticipants();

        var hubConnection = CreateHubConnection(roomId);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinRoomAsModerator", roomId);

        var voteCastTask = new TaskCompletionSource<string>();
        hubConnection.On<string>("OnVoteCast", name => { voteCastTask.SetResult(name); });

        var startSessionResponse =
            await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        if (startSessionResponse.StatusCode != HttpStatusCode.OK)
        {
            var errText = await startSessionResponse.Content.ReadAsStringAsync();
            throw new Exception($"Start session failed: {startSessionResponse.StatusCode}. Content: {errText}");
        }

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        var votePayload = new { participantName = "Ana", cardValue = "5" };
        var voteResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote", votePayload);
        voteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var voteCastName = await voteCastTask.Task;
        voteCastName.ShouldBe("Ana");

        var getResponse = await _httpClient.GetAsync($"v1/rooms/{roomId}/session");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var sessionDto = await getResponse.Content.ReadFromJsonAsync<EstimationSessionDto>();
        sessionDto.ShouldNotBeNull();
        sessionDto.Votes.Count.ShouldBe(1);
        sessionDto.Votes.ShouldContain(v => v.Name == "Ana" && v.Card == null);

        await hubConnection.DisposeAsync();
    }

    [Fact]
    public async Task Scenario9_SimultaneousRevealAndConsensusCheck()
    {
        var roomId = await SetupRoomWithParticipants();

        var hubConnection = CreateHubConnection(roomId);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinRoomAsModerator", roomId);

        var revealTask = new TaskCompletionSource<EstimationSessionDto>();
        hubConnection.On<EstimationSessionDto>("OnVotesRevealed", dto => { revealTask.SetResult(dto); });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Ana", cardValue = "5" });
        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Bob", cardValue = "5" });

        var revealResponse = await _httpClient.PostAsync($"v1/rooms/{roomId}/session/reveal", null);
        revealResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var revealedDto = await revealTask.Task;
        revealedDto.ConsensusValue.ShouldBe("5");
        revealedDto.HasDiscrepancy.ShouldBeFalse();
        revealedDto.Votes.ShouldContain(v => v.Name == "Ana" && v.Card == "5");
        revealedDto.Votes.ShouldContain(v => v.Name == "Bob" && v.Card == "5");

        await hubConnection.DisposeAsync();
    }

    [Fact]
    public async Task Scenario10_SimultaneousRevealWithDiscrepanciesRequiresReestimation()
    {
        var roomId = await SetupRoomWithParticipants();

        var hubConnection = CreateHubConnection(roomId);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinRoomAsModerator", roomId);

        var restartTask = new TaskCompletionSource<bool>();
        hubConnection.On("OnVotesRestarted", () => { restartTask.SetResult(true); });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Ana", cardValue = "3" });
        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Bob", cardValue = "5" });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/reveal", null);

        var restartResponse = await _httpClient.PostAsync($"v1/rooms/{roomId}/session/restart", null);
        restartResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var restarted = await restartTask.Task;
        restarted.ShouldBeTrue();

        var getResponse = await _httpClient.GetAsync($"v1/rooms/{roomId}/session");
        var sessionDto = await getResponse.Content.ReadFromJsonAsync<EstimationSessionDto>();
        sessionDto.ShouldNotBeNull();
        sessionDto.CurrentState.ShouldBe("PrivateEstimation");
        sessionDto.Votes.ShouldBeEmpty();

        await hubConnection.DisposeAsync();
    }

    [Fact]
    public async Task Scenario11_ProductOwnerAttemptsToVote()
    {
        var roomId = await SetupRoomWithParticipants();

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        var votePayload = new { participantName = "Pete", cardValue = "5" };
        var voteResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote", votePayload);

        voteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Scenario12_DeveloperVotesWithAxeCard()
    {
        var roomId = await SetupRoomWithParticipants();

        var hubConnection = CreateHubConnection(roomId);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinRoomAsModerator", roomId);

        var haltedTask = new TaskCompletionSource<string>();
        hubConnection.On<string>("OnSessionHalted", reason => { haltedTask.SetResult(reason); });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Ana", cardValue = "Axe" });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/reveal", null);

        var reason = await haltedTask.Task;
        reason.ShouldContain("too complex");

        var getResponse = await _httpClient.GetAsync($"v1/rooms/{roomId}/session");
        var sessionDto = await getResponse.Content.ReadFromJsonAsync<EstimationSessionDto>();
        sessionDto.ShouldNotBeNull();
        sessionDto.CurrentState.ShouldBe("Halted");
        sessionDto.FlaggedSpecialCards.ShouldContain("Axe");

        await hubConnection.DisposeAsync();
    }

    [Fact]
    public async Task Scenario13_DeveloperVotesWithOtherSpecialCards()
    {
        var roomId = await SetupRoomWithParticipants();

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/start?storyDescription=TestStory", null);
        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/transition/private-estimation", null);

        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Ana", cardValue = "Diagram" });
        await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/session/vote",
            new { participantName = "Bob", cardValue = "AI" });

        await _httpClient.PostAsync($"v1/rooms/{roomId}/session/reveal", null);

        var getResponse = await _httpClient.GetAsync($"v1/rooms/{roomId}/session");
        var sessionDto = await getResponse.Content.ReadFromJsonAsync<EstimationSessionDto>();
        sessionDto.ShouldNotBeNull();
        sessionDto.FlaggedSpecialCards.ShouldContain("Diagram");
        sessionDto.FlaggedSpecialCards.ShouldContain("AI");
    }

    private async Task<Guid> SetupRoomWithParticipants()
    {
        var createResponse = await _httpClient.PostAsync("v1/rooms?moderatorName=Carlos", null);
        var roomData = await createResponse.Content.ReadFromJsonAsync<RoomCreateResponse>();
        var roomId = roomData!.RoomId;

        await ApproveParticipant(roomId, "Ana", "Developer");
        await ApproveParticipant(roomId, "Bob", "Developer");
        await ApproveParticipant(roomId, "Pete", "Product Owner");

        return roomId;
    }

    private async Task ApproveParticipant(Guid roomId, string name, string role)
    {
        var joinPayload = new { name, role };
        var joinResponse = await _httpClient.PostAsJsonAsync($"v1/rooms/{roomId}/join-requests", joinPayload);
        var requestData = await joinResponse.Content.ReadFromJsonAsync<RoomJoinRequestResponse>();
        var requestId = requestData!.RequestId;

        await _httpClient.PostAsync($"v1/rooms/{roomId}/join-requests/{requestId}/approve", null);
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